namespace PaintingProjectsManagement.Blazor.Models;

public class Model
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Franchise { get; set; } = string.Empty;
    public string[] Characters { get; set; } = [];
    public int Size { get; set; }
    public EntityReference Category { get; set; } = new();
    public ModelType Type { get; set; }
    public string? Artist { get; set; }
    public string[] Tags { get; set; } = [];
    public string? PictureUrl { get; set; }
    public int Score { get; set; }
    public BaseSize BaseSize { get; set; } = new();
    public FigureSize FigureSize { get; set; } = new();
    public int NumberOfFigures { get; set; }
    public bool MustHave { get; set; }
}

public class EntityReference
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public EntityReference() { }
    public EntityReference(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
}

public enum ModelType
{
    // Add actual enum values from backend
}

public class BaseSize
{
    // Add properties based on backend model
}

public class FigureSize
{
    // Add properties based on backend model
} 