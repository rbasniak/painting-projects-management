namespace PaintingProjectsManagement.Features.Projects.Tests;

public class TestingServer : RbkTestingServer<Program>
{
    protected override bool UseHttps => true;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
    }
}
