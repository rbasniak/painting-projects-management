namespace PaintingProjectsManagement.Features.Projects;

public class ColorSection : BaseEntity
{
    public ColorZone Zone { get; private set; }
    public string ReferenceColor { get; private set; } = string.Empty;
    public Guid[] SuggestedColorIds { get; private set; } = Array.Empty<Guid>();
    public Guid UsedColorId { get; private set; } 

    public Guid ColorGroupId { get; private set; }
    public ColorGroup ColorGroup { get; private set; }
}