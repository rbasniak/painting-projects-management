using Microsoft.Extensions.DependencyInjection;

namespace Demo1.Tests;

public class Demo1TestingServer : RbkTestingServer<Program>
{
    protected override bool UseHttps => true;

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
    }
}
