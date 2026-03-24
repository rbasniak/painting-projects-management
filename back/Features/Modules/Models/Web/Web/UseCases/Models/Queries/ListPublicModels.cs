namespace PaintingProjectsManagement.Features.Models;

public sealed class ListPublicModels : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/models/public/{owner}", async (string owner, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request
            {
                Owner = owner
            }, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .Produces<System.Collections.ObjectModel.ReadOnlyCollection<ModelDetails>>(StatusCodes.Status200OK)
        .AllowAnonymous()
        .WithName("List Public Models")
        .WithTags("Models");
    }

    public sealed class Request : IQuery
    {
        public string Owner { get; set; } = string.Empty;
    }

    public sealed class Validator : AbstractValidator<Request>
    {
        public Validator()
        {
            RuleFor(x => x.Owner)
                .NotEmpty()
                .MaximumLength(120);
        }
    }

    public sealed class Handler(DbContext context) : IQueryHandler<Request>
    {
        private readonly DbContext _context = context;

        public async Task<QueryResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var owner = PublicModelsOwnerKeyCodec.DecodeOrFallback(request.Owner);
            var ownerUpper = owner.ToUpperInvariant();

            var models = await _context.Set<Model>()
                .Include(x => x.Category)
                .Where(x => x.TenantId == owner || x.TenantId == ownerUpper)
                .OrderBy(x => x.Category.Name)
                .ThenBy(x => x.Name)
                .ToListAsync(cancellationToken);

            var result = models
                .Select(ModelDetails.FromModel)
                .ToList()
                .AsReadOnly();

            return QueryResponse.Success(result);
        }
    }
}
