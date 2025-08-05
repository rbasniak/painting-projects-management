namespace PaintingProjectsManagement.Blazor.Modules.Shared;

public interface IModule
{
    string Name { get; }
    string Route { get; }
    string Icon { get; }
    int Order { get; }
}
