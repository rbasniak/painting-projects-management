﻿namespace rbkApiModules.Identity.Core;

public class DeleteRole : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapDelete("/api/authorization/roles/{id}", async (Guid id, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(new Request { Id = id }, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .RequireAuthorization()
        .WithName("Delete Role")
        .WithTags("Roles");
    }

    public class Request : AuthenticatedRequest, ICommand
    {
        public Guid Id { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IRolesService _rolesService;
        private readonly RbkAuthenticationOptions _authOptions;

        public Validator(IRolesService rolesService, ITenantsService tenantsService, ILocalizationService localization, RbkAuthenticationOptions authOptions)
        {
            _authOptions = authOptions;
            _rolesService = rolesService;

            RuleFor(x => x.Id)
                .RoleExistOnDatabaseForTheCurrentTenant(rolesService, localization)
                .MustAsync(NotBeUsedInAnyUserUnlessThereIsAnAlternateRole)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.RoleIsBeingUsed))
                .MustAsync(NotBeTheDefaultUserRole)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.CannotDeleteDefaultUserRole));

            RuleFor(x => x.Identity)
                .TenantExistOnDatabase(tenantsService, localization)
                .HasCorrectRoleManagementAccessRights(localization);
        }

        private async Task<bool> NotBeTheDefaultUserRole(Request request, Guid Id, CancellationToken cancellationToken)
        {
            if (!_authOptions._allowUserCreationOnFirstAccess && _authOptions._loginMode == LoginMode.Credentials ||
                !_authOptions._allowUserCreationOnFirstAccess && _authOptions._loginMode == LoginMode.Custom)
            {
                return true;
            }

            var role = await _rolesService.FindAsync(request.Id, cancellationToken);

            return role.Name.ToLower() != _authOptions._defaultRoleName.ToLower();
        }

        private async Task<bool> NotBeUsedInAnyUserUnlessThereIsAnAlternateRole(Request request, Guid id, CancellationToken cancellationToken)
        {
            var isUsed = await _rolesService.IsUsedByAnyUsersAsync(id, cancellationToken);

            // If it is used and is a tenant role, look for an application role to replace it
            if (isUsed && request.Identity.HasTenant)
            {
                var role = await _rolesService.FindAsync(id, cancellationToken);
                var roles = await _rolesService.FindByNameAsync(role.Name, cancellationToken);

                var applicationRole = roles.FirstOrDefault(x => x.HasNoTenant);

                if (_authOptions != null && _authOptions._defaultRoleName != null)
                {
                    return applicationRole != null && _authOptions._defaultRoleName.ToLower() != applicationRole.Name.ToLower();
                }
                else
                {
                    return applicationRole != null;
                }
            }
            else
            {
                return !isUsed;
            }
        } 
    }

    public class Handler : ICommandHandler<Request>
    {
        private readonly IRolesService _rolesService;
        private readonly IAuthService _usersService;

        public Handler(IRolesService rolesService, IAuthService usersService)
        {
            _rolesService = rolesService;
            _usersService = usersService;
        }

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var tenantRole = await _rolesService.FindAsync(request.Id, cancellationToken);

            if (request.Identity.HasTenant)
            {
                var existingRoles = await _rolesService.FindByNameAsync(tenantRole.Name, cancellationToken);
                var applicationRole = existingRoles.FirstOrDefault(x => x.TenantId == null);

                if (applicationRole != null)
                {
                    var users = await _usersService.GetAllWithRoleAsync(
                        userTenant: request.Identity.Tenant,
                        roleTenant: request.Identity.Tenant, 
                        roleName: applicationRole.Name, 
                        cancellationToken
                    );

                    foreach (var user in users)
                    {
                        await _usersService.RemoveRole(user.Username, user.TenantId, tenantRole.Id, cancellationToken);
                        await _usersService.AddRole(user.Username, user.TenantId, applicationRole.Id, cancellationToken);
                    }
                }
            }

            await _rolesService.DeleteAsync(request.Id, cancellationToken);
            
            return CommandResponse.Success();
        }
    }
}