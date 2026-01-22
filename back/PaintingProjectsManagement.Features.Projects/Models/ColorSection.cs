namespace PaintingProjectsManagement.Features.Projects;

public class ColorSection : BaseEntity
{
    private ColorSection()
    {
        // EF Core constructor, don't remove it
    }

    public ColorSection(Guid colorGroupId, ColorZone zone, string referenceColor)
    {
        ColorGroupId = colorGroupId;
        Zone = zone;
        ReferenceColor = referenceColor;
        SuggestedColorsJson = "[]";
    }

    public ColorZone Zone { get; private set; }
    public string ReferenceColor { get; private set; } = string.Empty;
    public string SuggestedColorsJson { get; private set; } = "[]";
    public Guid UsedColorId { get; private set; } 

    public Guid ColorGroupId { get; private set; }
    public ColorGroup ColorGroup { get; private set; }

    public void UpdateReferenceColor(string referenceColor)
    {
        if (string.IsNullOrWhiteSpace(referenceColor))
        {
            throw new ArgumentException("Reference color cannot be null or empty.", nameof(referenceColor));
        }

        ReferenceColor = referenceColor;
    }

    public void UpdateSuggestedColors(string suggestedColorsJson)
    {
        SuggestedColorsJson = suggestedColorsJson ?? "[]";
    }

    public void SetPickedColor(Guid paintColorId)
    {
        UsedColorId = paintColorId;
    }
}