namespace PaintingProjectsManagement.UI.Modules.Shared;

public interface IModule
{
    string Name { get; }

    string Route { get; }

    string Icon { get; }

    int Order { get; }

    IModuleRoute[] Routes { get; }
}

public interface IModuleRoute
{
    string Name { get; }

    string Route { get; }

    string Icon { get; }

    int Order { get; }
}

public sealed record ModuleRoute : IModuleRoute
{
    public required string Name { get; init; }

    public required string Route { get; init; }

    public required string Icon { get; init; }

    public int Order { get; init; }
} 