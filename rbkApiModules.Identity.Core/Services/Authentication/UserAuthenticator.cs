﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using rbkApiModules.Commons.Core;

namespace rbkApiModules.Identity.Core;

public interface IUserAuthenticator
{
    Task<JwtResponse> Authenticate(string username, string tenant, CancellationToken cancellationToken);
}

public class UserAuthenticator : IUserAuthenticator
{
    private readonly IAuthService _authService;
    private readonly IJwtFactory _jwtFactory;
    private readonly JwtIssuerOptions _jwtOptions;
    private readonly IEnumerable<ICustomClaimHandler> _claimHandlers;
    private readonly IAutomaticUserCreator _automaticUserCreator;
    private readonly ILogger<UserAuthenticator> _logger;

    public UserAuthenticator(IJwtFactory jwtFactory, IOptions<JwtIssuerOptions> jwtOptions, IAuthService authService,
    IEnumerable<ICustomClaimHandler> claimHandlers, IAutomaticUserCreator automaticUserCreator, ILogger<UserAuthenticator> logger)
    {
        _jwtFactory = jwtFactory;
        _authService = authService;
        _jwtOptions = jwtOptions.Value;
        _claimHandlers = claimHandlers;
        _automaticUserCreator = automaticUserCreator;
        _logger = logger;
    }

    public async Task<JwtResponse> Authenticate(string username, string tenant, CancellationToken cancellationToken)
    {
        var user = await _authService.FindUserAsync(username, tenant, cancellationToken);

        if (user == null)
        {
            user = await _automaticUserCreator.CreateIfAllowedAsync(username, tenant, cancellationToken);
        }

        _logger.LogInformation($"Loging in with user {user.Username}");

        if (user.RefreshTokenValidity < DateTime.UtcNow)
        {
            var refreshToken = Guid.NewGuid().ToString().ToLower().Replace("-", "");

            await _authService.UpdateRefreshTokenAsync(username, tenant, refreshToken, _jwtOptions.RefreshTokenLife, cancellationToken);
        }

        user = await _authService.GetUserWithDependenciesAsync(username, tenant, cancellationToken);

        var extraClaims = new Dictionary<string, string[]>
            {
                { JwtClaimIdentifiers.AuthenticationMode, new[] { user.AuthenticationMode.ToString() } }
            };

        _logger.LogInformation($"Token generated with AuthenticationMode={user.AuthenticationMode}");

        foreach (var claimHandler in _claimHandlers)
        {
            foreach (var claim in await claimHandler.GetClaims(user.TenantId, user.Username))
            {
                extraClaims.Add(claim.Type, new[] { claim.Value });
            }
        }

        var jwt = await TokenGenerator.GenerateAsync(_jwtFactory, user, extraClaims, cancellationToken);

        await _authService.RefreshLastLogin(username, tenant, cancellationToken);

        return jwt;
    }
}