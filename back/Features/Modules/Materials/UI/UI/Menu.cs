using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Materials;

public class Menu : IModule
{
    public string Name => "Materials";

    public string Route => "materials";

    public string Icon => "icon";

    public int Order => 1;
    public IModuleRoute[] Routes => [

        new ModuleRoute
        {
            Name = "Library",
            Route = "library",
            Icon = "icon"
        },
    ];
}
