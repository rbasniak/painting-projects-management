using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Projects;

public class Menu : IModule
{
    public string Name => "Projects";

    public string Route => "projects";

    public string Icon => "icon";

    public int Order => 1;
    public IModuleRoute[] Routes => [
    
        new ModuleRoute
        {
            Name = "My Projects",
            Route = "my-projects",
            Icon = "icon" 
        },
    ];
} 