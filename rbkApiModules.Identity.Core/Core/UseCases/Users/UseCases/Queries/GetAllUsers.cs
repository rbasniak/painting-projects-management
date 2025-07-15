
using rbkApiModules.Identity.Core.DataTransfer.Users;

namespace rbkApiModules.Identity.Core;

public class GetAllUsers : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/authorization/users", async (IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return Results.Ok(result);
        })
        .RequireAuthorization(AuthenticationClaims.MANAGE_USERS)
        .WithName("Get All Users")
        .WithTags("Authorization");
    }

    public class Request : AuthenticatedRequest, IQuery<IReadOnlyCollection<UserDetails>>
    {
    }

    public class Handler(IAuthService _usersService) : IQueryHandler<Request, IReadOnlyCollection<UserDetails>>
    {

        public async Task<QueryResponse<IReadOnlyCollection<UserDetails>>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var users = await _usersService.GetAllAsync(request.Identity.Tenant, cancellationToken);

            return QueryResponse.Success(users.Select(UserDetails.FromModel).AsReadOnly());
        }
    }
}