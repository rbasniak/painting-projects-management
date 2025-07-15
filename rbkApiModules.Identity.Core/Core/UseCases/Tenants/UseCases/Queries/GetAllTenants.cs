﻿namespace rbkApiModules.Identity.Core;

public class GetAllTenants
{
    public static void MapEndpointAnonymous(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/authorization/tenants", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName("Get All Tenants")
        .WithTags("Tenants");
    }

    public static void MapEndpointAuthenticated(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/authorization/tenants", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
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

    public class Handler(ITenantsService _tenantsService) : IQueryHandler<Request, IReadOnlyCollection<TenantDetails>>
    {

        public async Task<QueryResponse<IReadOnlyCollection<TenantDetails>>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var results = await _tenantsService.GetAllAsync(cancellationToken);

            return QueryResponse.Success(results.Select(TenantDetails.FromModel).AsReadOnly());
        }
    }
}
