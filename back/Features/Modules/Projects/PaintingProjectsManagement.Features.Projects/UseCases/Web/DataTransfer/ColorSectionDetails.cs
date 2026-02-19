using System.Text.Json;

namespace PaintingProjectsManagement.Features.Projects;

public class ColorMatchDetails
{
    public Guid PaintColorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string HexColor { get; set; } = string.Empty;
    public string BrandName { get; set; } = string.Empty;
    public string LineName { get; set; } = string.Empty;
    public double Distance { get; set; }
}

public class ColorSectionDetails
{
    public Guid Id { get; set; }
    public ColorZone Zone { get; set; }
    public string ReferenceColor { get; set; } = string.Empty;
    public ColorMatchDetails[] SuggestedColors { get; set; } = Array.Empty<ColorMatchDetails>();
    public Guid? PickedColorId { get; set; }

    public static ColorSectionDetails FromModel(ColorSection section)
    {
        ColorMatchDetails[] suggestedColors = Array.Empty<ColorMatchDetails>();
        
        if (!string.IsNullOrWhiteSpace(section.SuggestedColorsJson) && section.SuggestedColorsJson != "[]")
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                suggestedColors = JsonSerializer.Deserialize<ColorMatchDetails[]>(section.SuggestedColorsJson, options) 
                    ?? Array.Empty<ColorMatchDetails>();
            }
            catch
            {
                // If deserialization fails, return empty array
                suggestedColors = Array.Empty<ColorMatchDetails>();
            }
        }

        return new ColorSectionDetails
        {
            Id = section.Id,
            Zone = section.Zone,
            ReferenceColor = section.ReferenceColor,
            SuggestedColors = suggestedColors,
            PickedColorId = section.UsedColorId != Guid.Empty ? section.UsedColorId : null
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