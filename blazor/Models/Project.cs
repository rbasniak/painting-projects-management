namespace PaintingProjectsManagement.Blazor.Models;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;
    public DateTime? EndDate { get; set; }
} 