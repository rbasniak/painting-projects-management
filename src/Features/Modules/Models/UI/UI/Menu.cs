using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Models;

public class Menu : IModule
{
    public string Name => "Models";
    public string Route => "models";
    public string Icon => "icon";
    public int Order => 2;
    public IModuleRoute[] Routes => [
        new ModuleRoute
        {
            Name = "Library",
            Route = "library",
            Icon = "icon",
            Order = 1
        },
        new ModuleRoute
        {
            Name = "Classification",
            Route = "classification",
            Icon = "icon",
            Order = 2
        },
        new ModuleRoute
        {
            Name = "Categories",
            Route = "categories",
            Icon = "icon",
            Order = 3
        }
    ];
}
