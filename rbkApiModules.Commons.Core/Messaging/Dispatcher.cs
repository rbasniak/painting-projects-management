using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using rbkApiModules.Commons.Core.Helpers;
using System;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace rbkApiModules.Commons.Core;

public interface IDispatcher
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken) where TResponse : BaseResponse;

    Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification;
}

public class Dispatcher(IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
    : IDispatcher
{
    public async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken) where TResponse : BaseResponse
    {
        var commandType = request.GetType();
        var logger = serviceProvider.GetService(typeof(ILogger<>).MakeGenericType(commandType)) as ILogger;

        var commandTypeName = commandType.FullName.Split(".").Last().Replace("+", ".");

        logger.LogInformation("Executing command {CommandType}", commandTypeName);

        var ounterStopwatch = Stopwatch.StartNew();

        try
        {
            PropagateAuthenticatedUser(ref request);

            var validationResult = await ValidateAsync(logger, commandType, request, cancellationToken);

            if (validationResult.Count > 0)
            {
                var errorResponse = CommandResponseFactory.CreateFailed(request, new ValidationProblemDetails
                {
                    Title = "Validation Failed",
                    Detail = "One or more validation errors occurred.",
                    Status = StatusCodes.Status400BadRequest,
                    Errors = validationResult
                });

                return (TResponse)errorResponse;
            }

            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
            var handler = serviceProvider.GetService(handlerType);
            if (handler == null)
            {
                var errorResponse = CommandResponseFactory.CreateFailed(request, new ProblemDetails
                {
                    Title = "Handler Not Found",
                    Detail = $"No handler registered for request type {request.GetType().FullName}",
                    Status = StatusCodes.Status500InternalServerError
                });

                return (TResponse)errorResponse;
            }

            var behaviors = serviceProvider.GetServices(typeof(IPipelineBehavior<,>)
                .MakeGenericType(request.GetType(), typeof(TResponse)))
                .Cast<object>().ToList();

            Func<Task<TResponse>> handle = async () =>
            {
                var handleStopwatch = Stopwatch.StartNew();
                logger.LogDebug("Starting to handle command {CommandType}", commandTypeName);

                dynamic handler = serviceProvider.GetRequiredService(handlerType);

                var response = await handler.HandleAsync((dynamic)request, cancellationToken);

                handleStopwatch.Stop();

                logger.LogDebug("Command {CommandType} handled successfully in {ElapsedMilliseconds}ms", commandTypeName, handleStopwatch.ElapsedMilliseconds);

                return response;
            };

            // Pipeline behavior execution (in reverse order like MediatR)
            foreach (var behavior in behaviors.Reverse<object>())
            {
                var next = handle;
                handle = async () =>
                {
                    logger.LogDebug("Starting pipeline behavior {Behavior}", behavior.GetType().Name);

                    var behaviorStopwatch = Stopwatch.StartNew();

                    var method = behavior.GetType().GetMethod("Handle");
                    var response = await (Task<TResponse>)method.Invoke(behavior, new object[] { request, cancellationToken, next });

                    behaviorStopwatch.Stop();
                    logger.LogDebug("Behavior {Behavior} finished in {duration}ms", behavior.GetType().Name, behaviorStopwatch.ElapsedMilliseconds);

                    return response;
                };
            }

            return await handle();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing command {CommandType}", commandTypeName);
            
            var message = TestingEnvironmentChecker.IsTestingEnvironment ? ex.ToString() : ex.Message;
            var errorResponse = CommandResponseFactory.CreateFailed(request, new ProblemDetails
            {
                Title = "Command Execution Failed",
                Detail = message,
                Status = StatusCodes.Status500InternalServerError
            });

            return (TResponse)errorResponse;
        } 
        finally
        {
            ounterStopwatch.Stop();
            logger.LogInformation("Command {CommandType} execution completed in {ElapsedMilliseconds}ms", commandTypeName, ounterStopwatch.ElapsedMilliseconds);
        }
    }

    public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(typeof(TNotification));
        var handlers = serviceProvider.GetServices(handlerType).Cast<object>().ToList();

        foreach (var handler in handlers)
        {
            await (Task)handlerType.GetMethod("Handle").Invoke(handler, new object[] { notification, cancellationToken });
        }
    }

    private async Task<Dictionary<string, string[]>> ValidateAsync(ILogger logger, Type commandType, object request, CancellationToken cancellationToken)
    {
        var commandTypeName = commandType.FullName.Split(".").Last().Replace("+", ".");

        var validatorBaseType = typeof(AbstractValidator<>).MakeGenericType(commandType);

        var validator = serviceProvider.GetService(validatorBaseType);

        var errorSummary = new Dictionary<string, string[]>();

        if (validator is IValidator concreteValidator)
        {
            var sw = Stopwatch.StartNew();

            var validatorType = validator.GetType().FullName.Split(".").Last().Replace("+", ".");

            logger.LogDebug("Validating command {CommandType} using {Validator}", commandTypeName, validatorType);

            var context = new ValidationContext<object>(request);
            var result = await concreteValidator.ValidateAsync(context, cancellationToken);
            if (!result.IsValid)
            {
                foreach (var error in result.Errors)
                {
                    if (error.ErrorMessage == "ignore me!")
                    {
                        continue;
                    }

                    if (!errorSummary.ContainsKey(error.PropertyName))
                    {
                        errorSummary[error.PropertyName] = [error.ErrorMessage];
                    }
                    else
                    {
                        errorSummary[error.PropertyName] = errorSummary[error.PropertyName].Append(error.ErrorMessage).ToArray();
                    }
                } 
            }

            logger.LogDebug("Validator {Validator} finished in {duration}ms", validatorType, sw.ElapsedMilliseconds);
        }

        return errorSummary;
    }

    private void PropagateAuthenticatedUser<TResponse>(ref IRequest<TResponse> request)
    {
        if (request is IAuthenticatedRequest authenticatedRequest)
        {
            var user = httpContextAccessor.HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                var claims = user.Claims
                    .Where(x => x.Type == JwtClaimIdentifiers.Roles)
                    .Select(x => x.Value)
                    .ToArray();

                authenticatedRequest.SetIdentity(httpContextAccessor.GetTenant(), httpContextAccessor.GetUsername(), claims);
            }
        }
    }
}


public class CommandResponseFactory
{
    public static BaseResponse CreateFailed<T>(T request, ProblemDetails problemDetails) where T : class
    {
        if (request is IQuery)
        {
            return QueryResponse.Failure(problemDetails);
        }
        else if (request is ICommand)
        {
            return CommandResponse.Failure(problemDetails);
        }
        else
        {
            throw new NotSupportedException($"Request type {request.GetType().FullName} is not supported for command response creation.");
        }
    }
}