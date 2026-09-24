using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Subscriptions;

public sealed class Menu : IModule
{
    public string Name => "Subscriptions";
    public string Route => "subscriptions";
    public string Icon => "icon";
    public int Order => 99;

    public IModuleRoute[] Routes => [
        new ModuleRoute
        {
            Name = "Manage",
            Route = "manage",
            Icon = "icon",
            Order = 1
        }
    ];
}
