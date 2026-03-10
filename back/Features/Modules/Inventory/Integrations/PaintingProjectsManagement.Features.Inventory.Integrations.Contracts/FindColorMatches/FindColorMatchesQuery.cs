using System.Collections.Generic;

namespace PaintingProjectsManagement.Features.Inventory.Integration;

public class FindColorMatchesQuery : AuthenticatedRequest, IQuery<IReadOnlyCollection<ColorMatchResult>>
{
    public string ReferenceColor { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 10;
    public IReadOnlyCollection<PaintType> IncludedPaintTypes { get; set; } = Array.Empty<PaintType>();
}
