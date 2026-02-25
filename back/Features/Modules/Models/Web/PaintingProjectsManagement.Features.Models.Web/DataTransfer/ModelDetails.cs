using PaintingProjectsManagement.Infrastructure.Common;

namespace PaintingProjectsManagement.Features.Models;

public class ModelDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Franchise { get; set; } = string.Empty;
    public string[] Characters { get; set; } = [];
    public int Size { get; set; } 
    public EntityReference Category { get; set; }
    public EnumReference Type { get; set; }
    public string? Artist { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string? CoverPicture { get; set; }
    public string[] Pictures { get; set; } = [];
    public int Score { get; set; }
    public EnumReference BaseSize { get; set; }
    public EnumReference FigureSize { get; set; }
    public int NumberOfFigures { get; set; }
    public bool MustHave { get; set; }

    public static ModelDetails FromModel(Model model)
    {
        return new ModelDetails
        {
            Id = model.Id,
            Name = model.Name,
            Franchise = model.Franchise,
            Characters = model.Characters,
            Size = model.SizeInMb,
            Category = new EntityReference(model.Category.Id, model.Category.Name),
            Type = new EnumReference(model.Type),
            Artist = model.Artist,
            Tags = model.Tags,
            CoverPicture = model.CoverPicture,
            Pictures = model.Pictures,
            Score = model.Score.Value,
            BaseSize = new EnumReference(model.BaseSize),
            FigureSize = new EnumReference(model.FigureSize),
            NumberOfFigures = model.NumberOfFigures,
            MustHave = model.MustHave
        };
    }
}
