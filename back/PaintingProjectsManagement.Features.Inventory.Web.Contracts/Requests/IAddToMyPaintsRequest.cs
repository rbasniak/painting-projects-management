namespace PaintingProjectsManagement.Features.Inventory.Web;

public interface IAddToMyPaintsRequest
{
    IReadOnlyList<Guid> PaintColorIds { get; }
}
