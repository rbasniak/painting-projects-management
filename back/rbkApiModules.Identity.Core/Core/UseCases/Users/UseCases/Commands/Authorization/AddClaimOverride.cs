﻿using rbkApiModules.Identity.Core.DataTransfer.Users;

namespace rbkApiModules.Identity.Core;

public class AddClaimOverride : IEndpoint
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/authorization/users/add-claims", async (Request request, IDispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return ResultsMapper.FromResponse(result);
        })
        .RequireAuthorization(AuthenticationClaims.OVERRIDE_USER_CLAIMS)
        .WithName("Add Claim Override")
        .WithTags("Authorization");
    }

    public class Request : AuthenticatedRequest, ICommand   
    {
        public string Username { get; set; } = string.Empty;
        public Guid[] ClaimIds { get; set; } = [];
        public ClaimAccessType AccessType { get; set; }
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IAuthService _authService;

        public Validator(IAuthService authService, IClaimsService claimsService, ILocalizationService localization)
        {
            _authService = authService;

            RuleFor(x => x.Username)
                .NotEmpty()
                .MustAsync(UserExistInDatabaseUnderTheSameTenant)
                .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.UserNotFound))
                .DependentRules(() =>
                {
                    RuleFor(x => x.ClaimIds)
                   .Must(HaveAtLeastOneItem)
                   .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.RoleListMustNotBeEmpty))
                   .DependentRules(() =>
                   {
                       RuleForEach(x => x.ClaimIds)
                           .ClaimExistOnDatabase(claimsService, localization);
                   });
                });
        } 

        private async Task<bool> UserExistInDatabaseUnderTheSameTenant(Request request, string username, CancellationToken cancellationToken)
        {
            return await _authService.FindUserAsync(username, request.Identity.Tenant, cancellationToken) != null;
        }

        private bool HaveAtLeastOneItem(Guid[] claimIds)
        {
            return claimIds != null && claimIds.Any();
        }
    }

    public class Handler : ICommandHandler<Request>
    {
        private readonly IAuthService _authService;
        private readonly IClaimsService _claimsService;

        public Handler(IAuthService authService, IClaimsService claimsService)
        {
            _authService = authService;
            _claimsService = claimsService;
        }

        public async Task<CommandResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            await _claimsService.AddClaimOverridesAsync(request.ClaimIds, request.Username, request.Identity.Tenant, request.AccessType, cancellationToken);

            var user = await _authService.GetUserWithDependenciesAsync(request.Username, request.Identity.Tenant, cancellationToken);

            return CommandResponse.Success(UserDetails.FromModel(user));
        }
    }
}