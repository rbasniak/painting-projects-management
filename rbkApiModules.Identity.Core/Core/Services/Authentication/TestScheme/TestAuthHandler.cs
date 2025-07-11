﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using rbkApiModules.Commons.Core.Helpers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace rbkApiModules.Identity.Core;

public class TestAuthHandlerOptions : AuthenticationSchemeOptions
{
}

public class MockedWindowsAuthenticationHandler : AuthenticationHandler<TestAuthHandlerOptions>
{
    public const string UserId = "UserId";
    public const string AuthenticationScheme = "TestScheme";

    public MockedWindowsAuthenticationHandler(
        IOptionsMonitor<TestAuthHandlerOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!TestingEnvironmentChecker.IsTestingEnvironment)
        {
            throw new ExpectedInternalException("This handler is only supported in testing environment");
        }

        var claims = new List<System.Security.Claims.Claim>();

        // Extract User ID from the request headers if it exists,
        // otherwise use the default User ID from the options.
        if (Context.Request.Headers.TryGetValue(UserId, out var userId))
        {
            claims.Add(new System.Security.Claims.Claim(ClaimTypes.Name, userId[0]));
        }
        else
        {
            return Task.FromResult(AuthenticateResult.Fail(new UnauthorizedAccessException("No UserId was specified in the request header (needed for integration tests)")));
        }

        var identity = new ClaimsIdentity(claims, AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

