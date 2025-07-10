namespace rbkApiModules.Identity.Core;

public class GetAllTenants : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/authorization/tenants", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return Results.Ok(result);
        })
        .RequireAuthorization(AuthenticationClaims.MANAGE_TENANTS)
        .WithName("Get All Tenants")
        .WithTags("Tenants");
    }

    public class Request : IQuery<IReadOnlyCollection<TenantDetails>>
    {
    }

    public class Handler : IQueryHandler<Request, IReadOnlyCollection<TenantDetails>>
    {
        private readonly ITenantsService _tenantsService;

        public Handler(ITenantsService tenantsService)
        {
            _tenantsService = tenantsService;
        }

        public async Task<IReadOnlyCollection<TenantDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var results = await _tenantsService.GetAllAsync(cancellationToken);

            return results.Select(TenantDetails.FromModel).AsReadOnly();
        }
    }
}
