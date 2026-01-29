using PaintingProjectsManagement.Features.Inventory;
using PaintingProjectsManagement.Features.Inventory.Web;
using PaintingProjectsManagement.Infrastructure.Common;

namespace PaintingProjectsManagement.UI.Modules.Inventory;

public class PaintColorDetails : IPaintColorDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public string? ManufacturerCode { get; set; }
    public double BottleSize { get; set; }

    public EntityReference Line { get; set; }

    public EntityReference Brand { get; set; }

    public PaintType Type { get; set; }
}
