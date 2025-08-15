using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.UI.Modules.Shared;


public interface IModule
{
    string Name { get; }

    string Route { get; }

    string Icon { get; }

    int Order { get; }
}
