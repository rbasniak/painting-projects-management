using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace PaintingProjectsManagement.Features.Projects.Tests;

public class TestingServer : RbkTestingServer<Program>
{
    protected override bool UseHttps => true;

    protected override Task InitializeApplicationAsync()
    {
        return Task.CompletedTask;
    }

    protected override void ConfigureAppConfiguration(WebHostBuilderContext context, IConfigurationBuilder config)
    {
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
    }

    protected override IEnumerable<KeyValuePair<string, string>> ConfigureInMemoryOverrides()
    {
        throw new NotImplementedException();
    }
}
