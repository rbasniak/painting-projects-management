using PaintingProjectsManagement.Features.Inventory.Web;

namespace PaintingProjectsManagement.UI.Modules.Inventory;

public class AddToMyPaintsRequest : IAddToMyPaintsRequest
{
    public IReadOnlyList<Guid> PaintColorIds { get; set; } = Array.Empty<Guid>();
}
