namespace PaintingProjectsManagement.Features.Models;

public class SetModelMustHave : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut("/api/models/must-have", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Set Model Must Have")
        .WithTags("Models");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; }
        public bool MustHave { get; set; }
    }

    public class Validator : SmartValidator<Request, Model>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Id)
                .MustAsync(async (request, id, cancellationToken) =>
                    await Context.Set<Model>().AnyAsync(x => x.Id == id && x.TenantId == request.Identity.Tenant, cancellationToken))
                .WithMessage("Model not found.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {
        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var model = await _context.Set<Model>()
                .FirstAsync(x => x.Id == request.Id && x.TenantId == request.Identity.Tenant, cancellationToken);

            model.SetMustHave(request.MustHave);

            await _context.SaveChangesAsync(cancellationToken);

            return CommandResponse.Success();
        }
    }
} 