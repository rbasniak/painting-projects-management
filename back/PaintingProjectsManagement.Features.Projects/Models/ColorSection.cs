namespace PaintingProjectsManagement.Features.Projects;

public class ColorSection
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public ColorZone Zone { get; private set; }
    public string Color { get; private set; } = string.Empty;
    public Guid[] SuggestedColorIds { get; private set; } = Array.Empty<Guid>();
    public Guid ColorGroupId { get; private set; }
    public ColorGroup ColorGroup { get; private set; }
}