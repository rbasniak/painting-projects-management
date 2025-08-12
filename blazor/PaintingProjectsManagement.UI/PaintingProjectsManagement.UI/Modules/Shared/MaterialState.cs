using PaintingProjectsManagement.UI.Modules.Materials;
using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.UI.Modules.Shared;

public sealed class MaterialState
{
    public IReadOnlyCollection<MaterialDetails> Materials { get; set; } = (IReadOnlyCollection<MaterialDetails>)new List<MaterialDetails>();

    public IReadOnlyCollection<EnumReference> UnitTypes { get; set; } = (IReadOnlyCollection<EnumReference>)new List<EnumReference>();
}
