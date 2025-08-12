namespace PaintingProjectsManagement.UI.Modules.Shared;

public sealed class ApplicationState
{
    public MaterialState MaterialState { get; set; } = new MaterialState();

    public AuthenticationState AuthenticationState { get; set; }
}