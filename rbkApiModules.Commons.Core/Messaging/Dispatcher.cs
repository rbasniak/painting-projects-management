using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace rbkApiModules.Commons.Core;
public class Dispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public Dispatcher(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken ct = default)
    {
        var commandType = command.GetType();
        var logger = _serviceProvider.GetService(typeof(ILogger<>).MakeGenericType(commandType)) as ILogger;
        
        logger.LogDebug("Executing command {CommandType}", commandType.Name);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var validatorBaseType = typeof(AbstractValidator<>).MakeGenericType(commandType);

            var validator = _serviceProvider.GetService(validatorBaseType);

            if (validator is IValidator concreteValidator)
            {
                logger.LogDebug("Validating command {CommandType}", commandType.Name);

                var context = new ValidationContext<object>(command);
                var result = await concreteValidator.ValidateAsync(context, ct);
                if (!result.IsValid)
                {
                    var errorSummary = new Dictionary<string, string[]>();
                    foreach (var error in result.Errors)
                    {
                        if (!errorSummary.ContainsKey(error.PropertyName))
                        {
                            errorSummary[error.PropertyName] = [error.ErrorMessage];
                        }
                        else
                        {
                            errorSummary[error.PropertyName] = errorSummary[error.PropertyName].Append(error.ErrorMessage).ToArray();
                        }
                    }

                    logger.LogWarning("Command validation failed for {CommandType}: {Errors}", commandType.Name, errorSummary);
                    throw new InternalValidationException(errorSummary);
                }
            }

            var handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResponse));
            dynamic handler = _serviceProvider.GetRequiredService(handlerType);
            var response = await handler.HandleAsync((dynamic)command, ct);
            
            stopwatch.Stop();
            logger.LogInformation("Command {CommandType} executed successfully in {ElapsedMilliseconds}ms", commandType.Name, stopwatch.ElapsedMilliseconds);
            
            return response;
        }
        catch (Exception ex) when (ex is not InternalException)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Error executing command {CommandType} after {ElapsedMilliseconds}ms", commandType.Name, stopwatch.ElapsedMilliseconds);
            throw new UnexpectedInternalException("Error during validation of the request", ex);
        }
    }

    public async Task SendAsync(ICommand request, CancellationToken ct = default)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(request.GetType());
        if (_serviceProvider.GetService(validatorType) is IValidator validator)
        {
            var context = new ValidationContext<object>(request);
            var result = await validator.ValidateAsync(context, ct);
            if (!result.IsValid)
            {
                throw new UnexpectedInternalException(string.Join("; ", result.Errors.Select(e => e.ErrorMessage), 400));
            }
        }

        var handlerType = typeof(ICommandHandler<,>);
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        await handler.HandleAsync((dynamic)request, ct);
    }

    public Task<TResponse> QueryAsync<TResponse>(IQuery<TResponse> query, CancellationToken ct = default)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));
        dynamic handler = _serviceProvider.GetRequiredService(handlerType);
        return handler.HandleAsync((dynamic)query, ct);
    }

    public async Task PublishAsync<TNotification>(TNotification notification, CancellationToken ct = default)
        where TNotification : INotification
    {
        var handlers = _serviceProvider.GetServices<INotificationHandler<TNotification>>();

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(notification, ct);
            }
            catch (Exception ex)
            {
                // Log and swallow to prevent crashing the request
                Console.WriteLine($"[Dispatcher] Notification handler failed: {ex.Message}");
            }
        }
    }
}


public interface ICommand { }
public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
    Task HandleAsync(TCommand request, CancellationToken cancellationToken = default);
}

public interface ICommand<TResponse> { }
public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    Task<TResponse> HandleAsync(TCommand request, CancellationToken cancellationToken = default);
}

public interface IQuery<TResponse> { }
public interface IQueryHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    Task<TResponse> HandleAsync(TQuery request, CancellationToken cancellationToken = default);
}

public interface INotification { }
public interface INotificationHandler<TNotification>
    where TNotification : INotification
{
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken = default);
}
