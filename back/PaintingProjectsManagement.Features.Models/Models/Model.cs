using rbkApiModules.Commons.Core.Database;
using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.Features.Models;

public class Model : TenantEntity
{
    // Constructor for EF Core, do not remove it or make it public
    [JsonConstructor]
    private Model() { }

    public Model(string tenant, string name, ModelCategory category, string[] characters, string franchise, ModelType type, string artist, string[] tags, 
        BaseSize baseSize, FigureSize figureSize, int numberOfFigures, int sizeInMb)
    {
        Name = name;
        Category = category;
        Artist = artist;
        Tags = tags;
        Characters = characters;
        Score = -1;
        BaseSize = baseSize;
        FigureSize = figureSize;
        NumberOfFigures = numberOfFigures;
        Priority = -1;  
        Type = type;
        Franchise = franchise;
        TenantId = tenant;
        SizeInMb = sizeInMb;
    }

    public string Name { get; private set; } = string.Empty;
    public string Franchise { get; private set; } = string.Empty;
    [JsonColumn]
    public string[] Characters { get; private set; } = [];
    public Guid CategoryId { get; private set; }
    public ModelCategory Category { get; private set; }
    public ModelType Type { get; private set; }
    public string? Artist { get; private set; }
    public string[] Tags { get; private set; } = Array.Empty<string>();
    public string? PictureUrl { get; private set; }
    public int Score { get; private set; }
    public BaseSize BaseSize { get; private set; }
    public FigureSize FigureSize { get; private set; }
    public int NumberOfFigures { get; private set; }
    public int Priority { get; private set; }
    public int SizeInMb { get; private set; } = 0;

    public void UpdateDetails(string name, ModelCategory category, string[] characters, string artist, string[] tags, 
        BaseSize baseSize, FigureSize figureSize, int numberOfFigures, string franchise, ModelType type, int sizeInMb)
    {
        Name = name;
        Category = category;
        Artist = artist;
        Characters = characters ?? Array.Empty<string>();   
        Tags = tags ?? Array.Empty<string>();
        BaseSize = baseSize;
        FigureSize = figureSize;
        NumberOfFigures = numberOfFigures > 0 ? numberOfFigures : 1;
        Franchise = franchise;
        Type = type;
        SizeInMb = sizeInMb;
    }
    
    public void UpdatePicture(string pictureUrl)
    {
        PictureUrl = pictureUrl;
    }

    internal void ResetPriority()
    {
        Priority = 0;
    }

    internal void UpdatePriority(int priority)
    {
        Priority = priority;
    }
}
