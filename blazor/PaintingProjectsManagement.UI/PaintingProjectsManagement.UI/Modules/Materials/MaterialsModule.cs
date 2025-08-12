using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Materials;

public class MaterialsModule : IModule
{
    public string Name => "Materials";

    public string Route => "materials";

    public string Icon => "icon";

    public int Order => 1;
}