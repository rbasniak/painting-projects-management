namespace rbkApiModules.Commons.Core;

public interface IRequestContext
{
    string TenantId { get; }
    string Username { get; }
    string CorrelationId { get; }
    string CausationId { get; }
    bool IsAuthenticated { get; }
    bool HasTenant { get; }
}

public sealed class RequestContext : IRequestContext
{
    private static readonly AsyncLocal<RequestContext?> _current = new();
    public static RequestContext Current => _current.Value ??= new RequestContext();

    public string TenantId { get; internal set; } = string.Empty;
    public string Username { get; internal set; } = string.Empty;
    public string CorrelationId { get; internal set; } = string.Empty;
    public string CausationId { get; internal set; } = string.Empty;
    public bool IsAuthenticated => string.IsNullOrEmpty(Username) == false;
    public bool HasTenant => string.IsNullOrEmpty(TenantId) == false;

    internal static void Set(RequestContext ctx) => _current.Value = ctx;
}