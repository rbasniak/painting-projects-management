namespace PaintingProjectsManagement.Blazor.Shared.Configuration;

public static class ApiConfiguration
{
    public const string BaseUrl = "https://localhost:7236";
    
    public static class Endpoints
    {
        public const string Materials = "/api/materials";
        public const string Models = "/api/models";
        public const string ModelCategories = "/api/models/categories";
        public const string Projects = "/api/projects";
    }
} 