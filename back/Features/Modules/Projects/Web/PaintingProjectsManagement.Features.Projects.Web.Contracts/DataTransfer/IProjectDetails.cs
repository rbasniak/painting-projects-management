namespace PaintingProjectsManagement.Features.Projects.Web.Contracts;

public interface IProjectDetails
{
    Guid Id { get; }
    string Name { get; }
    string PictureUrl { get; }
    DateTime? StartDate { get; }
    DateTime? EndDate { get; }
}
