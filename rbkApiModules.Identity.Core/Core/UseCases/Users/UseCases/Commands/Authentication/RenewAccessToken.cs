﻿using Microsoft.Extensions.Logging;

namespace rbkApiModules.Identity.Core;

public class RenewAccessToken : IEndpoint   
{
    public static void MapEndpoint(IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/authentication/refresh-token", async (Request request, Dispatcher dispatcher, CancellationToken cancellationToken) =>
        {
            var result = await dispatcher.SendAsync(request, cancellationToken);

            return Results.Ok(result);
        })
        .AllowAnonymous()
        .WithName("Renew Access Token")
        .WithTags("Authentication");
    }

    public class Request : ICommand<JwtResponse>
    {
        public string RefreshToken { get; set; } = string.Empty;
    }

    public class Validator : AbstractValidator<Request>
    {
        private readonly IAuthService _usersService;
        private readonly ILogger<Validator> _logger;
        public Validator(IAuthService usersService, ILocalizationService localization, ILogger<Validator> logger)
        {
            _usersService = usersService;
            _logger = logger;

            RuleFor(x => x.RefreshToken)
                .NotEmpty()
                .MustAsync(RefreshTokenExistOnDatabase)
                .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.RefreshTokenNotFound))
                .MustAsync(TokenMustBeWithinValidity)
                .WithMessage(localization.LocalizeString(AuthenticationMessages.Validations.RefreshTokenExpired));
        }

        public async Task<bool> TokenMustBeWithinValidity(Request comman, string refreshToken, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validation: Verifying refresh token validity");

            var isRefreshTokenValid = await _usersService.IsRefreshTokenValidAsync(refreshToken, cancellationToken);

            if (isRefreshTokenValid)
            {
                _logger.LogInformation("Refresh token is valid");
            }
            else
            {
                _logger.LogInformation("Refresh token is not valid");
            }

            return isRefreshTokenValid;
        }

        public async Task<bool> RefreshTokenExistOnDatabase(Request request, string refreshToken, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Validation: Checking if refresh token exists on database");

            var tokenExists = await _usersService.RefreshTokenExistsOnDatabaseAsync(refreshToken, cancellationToken);

            if (tokenExists)
            {
                _logger.LogInformation("Refresh token was found");
            }
            else
            {
                _logger.LogInformation("Refresh token was not found");
            }

            return tokenExists;
        }
    }

    public class Handler : ICommandHandler<Request, JwtResponse>
    {
        private readonly IJwtFactory _jwtFactory;
        private readonly IAuthService _usersService;
        private readonly IEnumerable<ICustomClaimHandler> _claimHandlers;
        private readonly ILogger<Handler> _logger;

        public Handler(IJwtFactory jwtFactory, IAuthService usersService, IEnumerable<ICustomClaimHandler> claimHandlers, ILogger<Handler> logger)
        {
            _jwtFactory = jwtFactory;
            _usersService = usersService;
            _claimHandlers = claimHandlers;
            _logger = logger;
        }

        public async Task<JwtResponse> HandleAsync(Request request, CancellationToken cancellationToken)
        {
            var user = await _usersService.GetUserFromRefreshtokenAsync(request.RefreshToken, cancellationToken);
            
            _logger.LogInformation($"Renewing access token for user {user.Username}");

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

            _logger.LogInformation($"New token is {jwt.AccessToken}");

            return jwt;
        }
    }
}
