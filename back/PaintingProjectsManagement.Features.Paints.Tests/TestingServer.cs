using PaintingProjectsManagement.Api;

namespace PaintingProjectsManagement.Features.Paints.Tests;

public class TestingServer : RbkTestingServer<Program>
{
    protected override bool UseHttps => true;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
    }
}
