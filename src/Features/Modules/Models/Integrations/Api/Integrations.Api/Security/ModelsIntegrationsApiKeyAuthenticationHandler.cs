using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using rbkApiModules.Identity.Core;

namespace PaintingProjectsManagement.Features.Models.Integrations.Api;

public sealed class ModelsIntegrationsApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly DbContext _context;

    public ModelsIntegrationsApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        DbContext context)
        : base(options, logger, encoder)
    {
        _context = context;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!TryGetApiKeyValues(out var values))
        {
            return AuthenticateResult.NoResult();
        }

        if (values.Count != 1)
        {
            return AuthenticateResult.Fail("Invalid API key header.");
        }

        var key = values[0]?.Trim();
        if (!Guid.TryParse(key, out var userId))
        {
            return AuthenticateResult.Fail("Invalid API key format.");
        }

        var user = await _context.Set<User>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId, Context.RequestAborted);

        if (user is null)
        {
            return AuthenticateResult.Fail("Invalid API key.");
        }

        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(System.Security.Claims.ClaimTypes.Name, user.Username),
            new("sub", user.Id.ToString()),
            new("username", user.Username),
            new("tenant", user.TenantId),
            new("tenantId", user.TenantId)
        };

        var identity = new System.Security.Claims.ClaimsIdentity(claims, ModelsIntegrationsApiAuthentication.SchemeName);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, ModelsIntegrationsApiAuthentication.SchemeName);

        return AuthenticateResult.Success(ticket);
    }

    private bool TryGetApiKeyValues(out Microsoft.Extensions.Primitives.StringValues values)
    {
        if (Request.Headers.TryGetValue(ModelsIntegrationsApiAuthentication.HeaderName, out values))
        {
            return true;
        }

        return Request.Headers.TryGetValue(ModelsIntegrationsApiAuthentication.LegacyHeaderName, out values);
    }
}
