namespace PaintingProjectsManagement.Features.Projects;

public class ColorSectionDetails
{
    public Guid Id { get; set; }
    public ColorZone Zone { get; set; }
    public string ReferenceColor { get; set; } = string.Empty;
    public Guid[] SuggestedColorIds { get; set; } = Array.Empty<Guid>();

    public static ColorSectionDetails FromModel(ColorSection section)
    {
        return new ColorSectionDetails
        {
            Id = section.Id,
            Zone = section.Zone,
            ReferenceColor = section.ReferenceColor,
            SuggestedColorIds = section.SuggestedColorIds
        };
    }
}

public class ColorGroupDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ColorSectionDetails[] Sections { get; set; } = Array.Empty<ColorSectionDetails>();

    public static ColorGroupDetails FromModel(ColorGroup group)
    {
        return new ColorGroupDetails
        {
            Id = group.Id,
            Name = group.Name,
            Sections = group.Sections
                .Select(ColorSectionDetails.FromModel)
                .ToArray()
        };
    }
}