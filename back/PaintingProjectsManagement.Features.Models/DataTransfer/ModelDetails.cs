namespace PaintingProjectsManagement.Features.Models;

public class ModelDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Artist { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string? PictureUrl { get; set; }
    public int Score { get; set; }
    public BaseSize BaseSize { get; set; }
    public FigureSize FigureSize { get; set; }
    public int NumberOfFigures { get; set; }
    public int Priority { get; set; }

    public static ModelDetails FromModel(Model model)
    {
        return new ModelDetails
        {
            Id = model.Id,
            Name = model.Name,
            CategoryId = model.CategoryId,
            CategoryName = model.Category?.Name ?? string.Empty,
            Artist = model.Artist,
            Tags = model.Tags,
            PictureUrl = model.PictureUrl,
            Score = model.Score,
            BaseSize = model.BaseSize,
            FigureSize = model.FigureSize,
            NumberOfFigures = model.NumberOfFigures,
            Priority = model.Priority
        };
    }
}