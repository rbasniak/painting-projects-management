using PaintingProjectsManagement.UI.Modules.Shared;

namespace PaintingProjectsManagement.UI.Modules.Models;

public record ModelDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Franchise { get; set; } = string.Empty;
    public string[] Characters { get; set; } = [];
    public int Size { get; set; }
    public EntityReference Category { get; set; } = new(Guid.Empty, string.Empty);
    public EnumReference Type { get; set; } = new(0, string.Empty);
    public string? Artist { get; set; }
    public string[] Tags { get; set; } = [];
    public string? CoverPicture { get; set; }
    public string[] Pictures { get; set; } = [];
    public int Score { get; set; }
    public EnumReference BaseSize { get; set; } = new(0, string.Empty);
    public EnumReference FigureSize { get; set; } = new(0, string.Empty);
    public int NumberOfFigures { get; set; }
    public bool MustHave { get; set; }
}
