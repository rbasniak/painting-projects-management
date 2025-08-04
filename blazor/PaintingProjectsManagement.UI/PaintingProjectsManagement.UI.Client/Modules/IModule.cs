namespace PaintingProjectsManagement.UI.Client.Modules;

public interface IModule
{
    string Name { get; }
    string Route { get; }
    string Icon { get; }
    int Order { get; }
} 