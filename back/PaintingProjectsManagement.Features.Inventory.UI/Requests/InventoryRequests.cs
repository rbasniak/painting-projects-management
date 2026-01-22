namespace PaintingProjectsManagement.UI.Modules.Inventory;

public class AddToMyPaintsRequest
{
    public IReadOnlyList<Guid> PaintColorIds { get; set; } = Array.Empty<Guid>();
}
