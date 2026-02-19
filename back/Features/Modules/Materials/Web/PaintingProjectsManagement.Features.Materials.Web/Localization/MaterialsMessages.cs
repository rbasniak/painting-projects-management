using System.ComponentModel;

namespace PaintingProjectsManagement.Features.Materials;

public class MaterialsMessages : ILocalizedResource
{
    public enum Create
    {
        [Description("A material with this name already exists.")] MaterialWithNameAlreadyExists,
    }
}
