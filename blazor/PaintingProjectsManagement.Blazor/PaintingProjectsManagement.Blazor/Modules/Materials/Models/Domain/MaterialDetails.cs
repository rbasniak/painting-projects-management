using PaintingProjectsManagement.Blazor.Modules.Shared;

namespace PaintingProjectsManagement.Blazor.Modules.Materials;

public class MaterialDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EnumReference Unit { get; set; } = new EnumReference(MaterialUnit.Unknown);
    public double PricePerUnit { get; set; }
}

