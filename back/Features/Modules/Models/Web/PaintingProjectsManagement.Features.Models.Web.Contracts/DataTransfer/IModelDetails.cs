namespace PaintingProjectsManagement.Features.Models.Web.Contracts;

public interface IModelDetails
{
    Guid Id { get; }
    string Name { get; }
    string Franchise { get; }
    string[] Characters { get; }
    int Size { get; }
    object Category { get; }
    object Type { get; }
    string? Artist { get; }
    string[] Tags { get; }
    string? CoverPicture { get; }
    string[] Pictures { get; }
    int Score { get; }
    object BaseSize { get; }
    object FigureSize { get; }
    int NumberOfFigures { get; }
    bool MustHave { get; }
}
