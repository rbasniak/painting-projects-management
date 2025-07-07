using Microsoft.AspNetCore.Routing;

namespace rbkApiModules.Commons.Core;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder endpoints);
}
