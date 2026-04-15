namespace PaintingProjectsManagement.Features.Models.Integrations.Api;

public class DeleteModel : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/integrations/models/delete", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);
            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization(ModelsIntegrationsApiAuthentication.PolicyName)
        .WithName("Integrations - Delete Model")
        .WithTags("Models Integrations");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public string Id { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.")
                .MaximumLength(512)
                .WithMessage("Id cannot exceed 512 characters.");
        }
    }

    public class Handler(DbContext context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var id = request.Id.Trim();
            var query = context.Set<Model>().Where(x => x.TenantId == request.Identity.Tenant);

            Model? model = null;
            if (Guid.TryParse(id, out var parsedId))
            {
                model = await query.FirstOrDefaultAsync(x => x.Id == parsedId, cancellationToken);
            }

            model ??= await query.FirstOrDefaultAsync(x => x.Identity == id, cancellationToken);

            if (model is null)
            {
                return CommandResponse.Success();
            }

            context.Remove(model);
            await context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
}
