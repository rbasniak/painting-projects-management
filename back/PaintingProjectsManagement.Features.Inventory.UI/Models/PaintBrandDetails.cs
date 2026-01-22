namespace PaintingProjectsManagement.UI.Modules.Inventory;

public class PaintBrandDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<PaintLineDetails> Lines { get; set; } = Array.Empty<PaintLineDetails>();
}
