using MudBlazor;

namespace PaintingProjectsManagement.UI.Client.Modules.Materials;

public class MaterialsModule : IModule
{
    public string Name => "Materials";
    public string Route => "materials";
    public string Icon => Icons.Material.Filled.Inventory;
    public int Order => 1;
} 