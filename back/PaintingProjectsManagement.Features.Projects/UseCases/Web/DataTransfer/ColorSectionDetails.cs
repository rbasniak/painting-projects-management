namespace PaintingProjectsManagement.Features.Projects;

public class ColorSectionDetails
{
    public Guid Id { get; set; }
    public ColorGroupDetails Group { get; set; } = new();
    public ColorZone Zone { get; set; }
    public string Color { get; set; } = string.Empty;
    public Guid[] SuggestedColorIds { get; set; } = Array.Empty<Guid>();

    public static ColorSectionDetails FromModel(ColorSection section)
    {
        return new ColorSectionDetails
        {
            Id = section.Id,
            Group = ColorGroupDetails.FromModel(section.ColorGroup),
            Zone = section.Zone,
            Color = section.ReferenceColor,
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