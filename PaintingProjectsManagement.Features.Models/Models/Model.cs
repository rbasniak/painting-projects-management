namespace PaintingProjectsManagement.Features.Models;

public class Model
{
    // Constructor for EF Core, do not remove it or make it public
    private Model() { }

    public Model(string name, ModelCategory category, string artist, string[] tags, 
        BaseSize baseSize, FigureSize figureSize,  int numberOfFigures)
    {
        Name = name;
        Category = category;
        CategoryId = category.Id;
        Artist = artist;
        Tags = tags;
        Score = -1;
        BaseSize = baseSize;
        FigureSize = figureSize;
        NumberOfFigures = numberOfFigures;
        Priority = -1;  
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid CategoryId { get; private set; }
    public ModelCategory Category { get; private set; }
    public string? Artist { get; private set; }
    public string[] Tags { get; private set; } = Array.Empty<string>();
    public string? PictureUrl { get; private set; }
    public int Score { get; private set; }
    public BaseSize BaseSize { get; private set; }
    public FigureSize FigureSize { get; private set; }
    public int NumberOfFigures { get; private set; }
    public int Priority { get; private set; }

    public void UpdateDetails(string name, ModelCategory category, string artist, string[] tags, 
        BaseSize baseSize, FigureSize figureSize, int numberOfFigures)
    {
        Name = name;
        Category = category;
        CategoryId = category.Id;
        Artist = artist;
        Tags = tags ?? Array.Empty<string>();
        BaseSize = baseSize;
        FigureSize = figureSize;
        NumberOfFigures = numberOfFigures > 0 ? numberOfFigures : 1;
    }
    
    public void UpdatePicture(string pictureUrl)
    {
        PictureUrl = pictureUrl;
    }
    
    public void ResetPriority(int priority)
    {
        Priority = priority;
    }

    public void ResetPriority()
    {
        Priority = -1;
    }
}
