namespace PaintingProjectsManagement.UI.Modules.Shared;

public class MaterialState
{
    public bool IsLoading { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
} 