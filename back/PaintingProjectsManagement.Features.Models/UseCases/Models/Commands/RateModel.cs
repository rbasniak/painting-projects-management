namespace PaintingProjectsManagement.Features.Models;

public class RateModel : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/models/rate", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<ModelDetails>(StatusCodes.Status200OK)
        .RequireAuthorization()
        .WithName("Rate Model")
        .WithTags("Models");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; }
        public int Score { get; set; }
    }

    public class Validator : SmartValidator<Request, Model>
    {
        public Validator(DbContext context, ILocalizationService localization) : base(context, localization)
        {
        }

        protected override void ValidateBusinessRules()
        {
            RuleFor(x => x.Score)
                .GreaterThanOrEqualTo(1)
                .WithMessage("Score must be a value between 1 and 5.")
                .LessThanOrEqualTo(5)
                .WithMessage("Score must be a value between 1 and 5.");
        }
    }

    public class Handler(DbContext _context) : ICommandHandler<Request>
    {

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var model = await _context.Set<Model>()
                .Where(x => x.Category.TenantId == request.Identity.Tenant)
                .Include(x => x.Category)
                .FirstAsync(x => x.Id == request.Id, cancellationToken);

            model.Rate(request.Score);

            await _context.SaveChangesAsync(cancellationToken);

            var result = ModelDetails.FromModel(model);

            return CommandResponse.Success(result);
        }
    }
}