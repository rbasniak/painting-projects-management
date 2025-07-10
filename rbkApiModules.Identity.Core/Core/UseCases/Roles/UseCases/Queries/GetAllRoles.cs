using rbkApiModules.Identity.Core.DataTransfer;

namespace rbkApiModules.Identity.Core;

public class GetAllRoles : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/authorization/roles", async (Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request(), cancellationToken);

            return Results.Ok(result);
        })
        .RequireAuthorization(AuthenticationClaims.MANAGE_USER_ROLES)
        .WithName("Get All Roles")
        .WithTags("Roles");
    }

    public class Request : AuthenticatedRequest, IQuery<IReadOnlyCollection<RoleDetails>>
    {
    }

    public class Handler : IQueryHandler<Request, IReadOnlyCollection<RoleDetails>>
    {
        private readonly IRolesService _rolesService;

        public Handler(IRolesService context)
        {
            _rolesService = context;
        }

        public async Task<IReadOnlyCollection<RoleDetails>> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var roles = await _rolesService.GetAllAsync(cancellationToken);

            List<Role> results;

            if (request.Identity.HasTenant)
            {
                results = roles.Where(x => x.HasTenant).ToList();

                foreach (var role in roles.Where(x => x.HasNoTenant))
                {
                    var roleAlreadyInResults = results.FirstOrDefault(x => x.Name.ToUpper() == role.Name.ToUpper());

                    if (roleAlreadyInResults == null)
                    {
                        role.SetMode(isOverwritten: false);
                        results.Add(role);
                    }
                    else
                    {
                        role.SetMode(isOverwritten: true);
                    }
                }

                results = results.OrderBy(x => x.Name).ToList();
            }
            else
            {
                results = roles.Where(x => x.HasNoTenant).ToList();

                foreach (var role in results)
                {
                    role.SetMode(isOverwritten: false);
                }
            }

            return results.Select(RoleDetails.FromModel).AsReadOnly();
        }
    }
}