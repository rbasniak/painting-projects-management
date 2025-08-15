using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Materials;

public class MaterialDetails
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public EnumReference Unit { get; set; } = new EnumReference((Enum)MaterialUnit.Unknown);

    public double PricePerUnit { get; set; }
}
