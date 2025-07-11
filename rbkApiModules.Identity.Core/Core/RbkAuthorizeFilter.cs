internal sealed class RbkAuthorizationFilter : IEndpointFilter
{
    private readonly string _requiredClaim;

    internal RbkAuthorizationFilter(string requiredClaim)
    {
        _requiredClaim = requiredClaim;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? false)
        {
            return Results.Unauthorized();
        }

        var roles = user.Claims
            .Where(c => c.Type == JwtClaimIdentifiers.Roles)
            .Select(c => c.Value)
            .ToList();

        if (!roles.Contains(_requiredClaim))
        {
            return Results.Forbid();
        }

        return await next(context);
    } 
}
