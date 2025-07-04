using Microsoft.AspNetCore.Routing;

namespace PaintingProjectsManagement.Infrastructure;

public interface IEndpoint
{
    static abstract void MapEndpoint(IEndpointRouteBuilder endpoints);
}
