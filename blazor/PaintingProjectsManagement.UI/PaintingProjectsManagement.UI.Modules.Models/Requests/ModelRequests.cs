namespace PaintingProjectsManagement.UI.Modules.Models;

public class CreateModelRequest
{
    public string Name { get; init; } = string.Empty;
    public string Franchise { get; init; } = string.Empty;
    public string[] Characters { get; init; } = [];
    public Guid CategoryId { get; init; }
    public ModelType Type { get; init; }
    public string? Artist { get; init; }
    public string[] Tags { get; init; } = [];
    public string? CoverPicture { get; init; }
    public BaseSize BaseSize { get; init; }
    public FigureSize FigureSize { get; init; }
    public int NumberOfFigures { get; init; }
    public int SizeInMb { get; init; }
}

public class UpdateModelRequest : CreateModelRequest
{
    public Guid Id { get; init; }
}

public class CreateModelCategoryRequest
{
    public string Name { get; init; } = string.Empty;
}

public class UpdateModelCategoryRequest : CreateModelCategoryRequest
{
    public Guid Id { get; init; }
}

public enum ModelType
{
    Unknown = 0,
    Miniature = 1,
    Vehicle = 2,
    Terrain = 3,
    Figure = 4,
    Bust = 5,
    Other = 99
}

public enum BaseSize
{
    Unknown = 0,
    Small = 10,
    Medium = 20,
    Big = 30
}

public enum FigureSize
{
    Unknown = 0,
    Normal = 10,
    Big = 20
}
