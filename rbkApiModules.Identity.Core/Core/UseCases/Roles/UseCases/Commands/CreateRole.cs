using rbkApiModules.Identity.Core.DataTransfer;

namespace rbkApiModules.Identity.Core;

public class CreateRole : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/authorization/roles", async (Request request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("Create Role")
        .WithTags("Roles");
    }

    public class Request : AuthenticatedRequest, ICommand<RoleDetails>
    {
        public string Name { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IRolesService _rolesService;

        public Validator(IRolesService rolesService, ITenantsService tenantsService, ILocalizationService localization)
        {
            _rolesService = rolesService;

            RuleFor(x => x.Name)
                .NotEmpty()
                .MustAsync(NameBeUnique)
                .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.NameAlreadyUsed));

            RuleFor(x => x.Identity)
                .TenantExistOnDatabase(tenantsService, localization)
                .HasCorrectRoleManagementAccessRights(localization);
        }  

        private async Task<bool> NameBeUnique(Request request, string name, CancellationToken cancellationToken)
        {
            var roles = await _rolesService.FindByNameAsync(name, cancellationToken);

            if (roles.Count() == 0) return true;

            var usedRoles = roles.Where(x => x.TenantId == request.Identity.Tenant || (x.HasNoTenant && request.Identity.HasNoTenant)).ToList();

            return usedRoles.Count == 0;
        }
    }

    public class Handler : ICommandHandler<Request, RoleDetails>
    {
        private readonly IRolesService _rolesService;
        private readonly IAuthService _usersService;

        public Handler(IRolesService rolesService, IAuthService usersService)
        {
            _rolesService = rolesService;
            _usersService = usersService;
        }

        public async Task<RoleDetails> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var tenantRole = new Role(request.Identity.Tenant, request.Name);

            tenantRole = await _rolesService.CreateAsync(tenantRole, cancellationToken);

            if (request.Identity.HasTenant)
            {
                var existingRoles = await _rolesService.FindByNameAsync(tenantRole.Name, cancellationToken);
                var applicationRole = existingRoles.FirstOrDefault(x => x.TenantId == null);

                if (applicationRole != null)
                {
                    var users = await _usersService.GetAllWithRoleAsync(
                        userTenant: request.Identity.Tenant, 
                        roleTenant: null, 
                        roleName: applicationRole.Name, 
                        cancellationToken);

                    foreach (var user in users)
                    {
                        await _usersService.AddRole(user.Username, user.TenantId, tenantRole.Id, cancellationToken);
                        await _usersService.RemoveRole(user.Username, user.TenantId, applicationRole.Id, cancellationToken);
                    }
                }
            }

            return RoleDetails.FromModel(tenantRole);
        }
    } 
}