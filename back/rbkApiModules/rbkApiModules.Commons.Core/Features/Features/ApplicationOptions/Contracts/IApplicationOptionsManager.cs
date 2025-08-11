using System.Security.Claims;

namespace rbkApiModules.Commons.Core.Features.ApplicationOptions;

public interface IApplicationOptionsManager
{
    TOptions GetOptions<TOptions>(string? tenantId = null, Guid? userId = null)
        where TOptions : class, IApplicationOptions, new();
}

