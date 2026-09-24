using PaintingProjectsManagement.Features.Inventory.Web;
using PaintingProjectsManagement.Infrastructure.Common;

namespace PaintingProjectsManagement.UI.Modules.Inventory;

public class PaintLineDetails : IPaintLineDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyList<PaintColorDetails> Paints { get; set; } = Array.Empty<PaintColorDetails>();

    public EntityReference Brand { get; set; }

    IReadOnlyList<IPaintColorDetails> IPaintLineDetails.Paints => Paints;
}
