using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace rbkApiModules.Identity.Core;

public class SwitchTenant : IEndpoint   
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/authentication/switch-tenant", async (Request request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .RequireAuthorization()
        .WithName("Switch Tenant")
        .WithTags("Users");
    }

    public class Request : AuthenticatedRequest, ICommand<JwtResponse>
    {
        public string Tenant { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IAuthService _usersService;
        private readonly ITenantsService _tenantsService;
        private readonly RbkAuthenticationOptions _authOptions;

        public Validator(IAuthService usersService, ITenantsService tenantsService, ILocalizationService localization, RbkAuthenticationOptions authOptions)
        {
            _authOptions = authOptions;
            _usersService = usersService;
            _tenantsService = tenantsService;

            RuleFor(x => x.Tenant)
                .NotEmpty()
                .MustAsync(ExistInDatabase)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.TenantNotFound))
                .MustAsync(BePartOfDomain)
                    .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.UnauthorizedAccess));
        }

        public async Task<bool> ExistInDatabase(Request request, string destinationDomain, CancellationToken cancellationToken)
        {
            return await _tenantsService.FindAsync(destinationDomain, cancellationToken) != null; 
        }

        public async Task<bool> BePartOfDomain(Request request, string destinationDomain, CancellationToken cancellationToken)
        {
            var userWillBeAutomaticallyCreated = _authOptions._allowUserCreationOnFirstAccess && _authOptions._loginMode == LoginMode.WindowsAuthentication ||
                _authOptions._allowUserCreationOnFirstAccess && _authOptions._loginMode == LoginMode.Custom;

            var userExists = await _usersService.FindUserAsync(request.Identity.Username, destinationDomain, cancellationToken) != null;

            return userExists || userWillBeAutomaticallyCreated;
        }
    }

    public class Handler : ICommandHandler<Request, JwtResponse>
    {
        private readonly IJwtFactory _jwtFactory;
        private readonly IAuthService _usersService;
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly IAutomaticUserCreator _automaticUserCreator;
        private readonly IEnumerable<ICustomClaimHandler> _claimHandlers;
        private readonly ILogger<Handler> _logger;

        public Handler(IJwtFactory jwtFactory, IAuthService usersService, IEnumerable<ICustomClaimHandler> claimHandlers, 
            IOptions<JwtIssuerOptions> jwtOptions, IAutomaticUserCreator automaticUserCreator, ILogger<Handler> logger)
        {
            _jwtFactory = jwtFactory;
            _usersService = usersService;
            _jwtOptions = jwtOptions.Value;
            _claimHandlers = claimHandlers;
            _automaticUserCreator = automaticUserCreator;
            _logger = logger;
        }

        public async Task<JwtResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var user = await _usersService.FindUserAsync(request.Identity.Username, request.Tenant, cancellationToken);

            if (user == null)
            {
                await _automaticUserCreator.CreateIfAllowedAsync(request.Identity.Username, request.Tenant, cancellationToken);
            }

            var refreshToken = Guid.NewGuid().ToString().ToLower().Replace("-", "");

            await _usersService.UpdateRefreshTokenAsync(request.Identity.Username, request.Tenant, refreshToken, _jwtOptions.RefreshTokenLife, cancellationToken);

            user = await _usersService.GetUserWithDependenciesAsync(request.Identity.Username, request.Tenant, cancellationToken);

            _logger.LogInformation($"Switching domain for user {user.Username}");

            var extraClaims = new Dictionary<string, string[]>
            {
                { JwtClaimIdentifiers.AuthenticationMode, new[] { user.AuthenticationMode.ToString() } }
            };

            foreach (var claimHandler in _claimHandlers)
            {
                foreach (var claim in await claimHandler.GetClaims(user.TenantId, user.Username))
                {
                    extraClaims.Add(claim.Type, new[] { claim.Value });
                }
            }

            var jwt = await TokenGenerator.GenerateAsync(_jwtFactory, user, extraClaims, cancellationToken);

            await _usersService.RefreshLastLogin(user.Username, user.TenantId, cancellationToken);

            return jwt;
        }
    }
}
