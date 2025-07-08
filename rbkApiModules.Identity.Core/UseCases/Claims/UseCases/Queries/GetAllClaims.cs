namespace rbkApiModules.Identity.Core;

public class GetAllClaims : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/authorization/claims", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.QueryAsync(new Request(), cancellationToken);

            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName("Get All Claims")
        .WithTags("Authorization");
    }

    public class Request : IQuery<IReadOnlyCollection<ClaimDetails>>
    {
    }

    public class Handler : IQueryHandler<Request, IReadOnlyCollection<ClaimDetails>>
    {
        private readonly IClaimsService _claimsService;

        public Handler(IClaimsService context)
        {
            _claimsService = context;
        }

        public async Task<IReadOnlyCollection<ClaimDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var claim = await _claimsService.GetAllAsync(cancellationToken);

            return claim.Select(ClaimDetails.FromModel).AsReadOnly();
        }
    }
}
