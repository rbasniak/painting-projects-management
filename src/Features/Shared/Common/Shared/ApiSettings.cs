namespace PaintingProjectsManagement.UI.Modules.Shared;

public class ApiSettings
{
    public Uri BaseUrl { get; }

    public ApiSettings(Uri baseUrl)
    {
        BaseUrl = baseUrl;
    }

    public string GetUrl(string path) =>
        $"{BaseUrl}{path.TrimStart('/')}";
}
