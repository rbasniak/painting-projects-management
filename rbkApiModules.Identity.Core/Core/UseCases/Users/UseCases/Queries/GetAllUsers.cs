
using rbkApiModules.Identity.Core.DataTransfer.Users;

namespace rbkApiModules.Identity.Core;

public class GetAllUsers : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/authorization/users", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
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

    public class Handler : IQueryHandler<Request, IReadOnlyCollection<UserDetails>>
    {
        private readonly IAuthService _usersService;

        public Handler(IAuthService usersService)
        {
            _usersService = usersService;
        }

        public async Task<IReadOnlyCollection<UserDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var users = await _usersService.GetAllAsync(request.Identity.Tenant, cancellationToken);

            return users.Select(UserDetails.FromModel).AsReadOnly();
        }
    }
}