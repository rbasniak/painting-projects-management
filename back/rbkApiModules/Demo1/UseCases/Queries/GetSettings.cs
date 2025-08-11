using Demo1.Models;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using rbkApiModules.Commons.Core.Features.ApplicationOptions;

namespace Demo1.UseCases.Commands;

public class GetSettings : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/options", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return Results.Ok(result.Data);
        })
        .WithName("Get Application Options")
        .WithTags("Options");
    }

    public class Request : ICommand
    {
    }

    public class Handler : ICommandHandler<Request>
    {
        private readonly IEnumerable<IApplicationOptions> _applicationOptions;

        public Handler(IEnumerable<IApplicationOptions> options)
        {
            _applicationOptions = options;
        }

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var option in _applicationOptions)
            {
                dictionary.Add(option.GetType().Name, option);
            }

            return CommandResponse.Success(dictionary);
        }
    }
} 