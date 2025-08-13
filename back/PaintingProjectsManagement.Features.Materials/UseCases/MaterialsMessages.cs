using System.ComponentModel;

namespace PaintingProjectsManagement.Features.Materials;

internal class MaterialsMessages : ILocalizedResource
{
    public enum Create
    {
        [Description("A material with this name already exists.")] MaterialWithNameAlreadyExists,
    }
}
