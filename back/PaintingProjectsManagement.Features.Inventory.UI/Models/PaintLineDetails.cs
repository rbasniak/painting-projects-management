namespace PaintingProjectsManagement.UI.Modules.Inventory;

public class PaintLineDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<PaintColorDetails> Paints { get; set; } = Array.Empty<PaintColorDetails>();
}
