using Microsoft.Extensions.Logging;
using rbkApiModules.Commons.Core;

namespace PaintingProjectsManagement.Features.Projects;

public sealed class ProjectsIntegrationConsumer : IntegrationConsumerBackgroundService
{
    protected override Dictionary<string, string[]> Subscriptions => new()
    {
        ["projects.materials"] = ["materials.*.v1"],
        ["projects.models"] = ["models.*.v1"]
    };

    public ProjectsIntegrationConsumer(
        IBrokerSubscriber subscriber,
        IServiceScopeFactory scopeFactory,
        IEventTypeRegistry eventTypeRegistry,
        ILogger<ProjectsIntegrationConsumer> logger)
        : base(subscriber, scopeFactory, eventTypeRegistry, logger)
    {
    }
}
