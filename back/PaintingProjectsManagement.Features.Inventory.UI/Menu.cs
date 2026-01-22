using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Inventory;

public class Menu : IModule
{
    public string Name => "Inventory";

    public string Route => "inventory";

    public string Icon => "icon";

    public int Order => 3;
    public IModuleRoute[] Routes => [
        new ModuleRoute
        {
            Name = "My Paints",
            Route = "my-paints",
            Icon = "icon",
            Order = 1
        },
        new ModuleRoute
        {
            Name = "Catalog",
            Route = "catalog",
            Icon = "icon",
            Order = 2
        },
    ];
}
