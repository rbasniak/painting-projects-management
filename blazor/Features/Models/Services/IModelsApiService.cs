using PaintingProjectsManagement.Blazor.Features.Models.Models;

namespace PaintingProjectsManagement.Blazor.Features.Models.Services;

public interface IModelsApiService
{
    Task<List<Model>> GetModelsAsync();
    Task<Model?> GetModelAsync(Guid id);
    Task<Model> CreateModelAsync(CreateModelRequest request);
    Task<Model> UpdateModelAsync(Guid id, UpdateModelRequest request);
    Task DeleteModelAsync(Guid id);
    Task RateModelAsync(Guid id, int score);
    Task SetMustHaveAsync(Guid id, bool mustHave);
}

public interface IModelCategoriesApiService
{
    Task<List<ModelCategory>> GetModelCategoriesAsync();
    Task<ModelCategory?> GetModelCategoryAsync(Guid id);
    Task<ModelCategory> CreateModelCategoryAsync(CreateModelCategoryRequest request);
    Task<ModelCategory> UpdateModelCategoryAsync(Guid id, UpdateModelCategoryRequest request);
    Task DeleteModelCategoryAsync(Guid id);
}

public class CreateModelRequest
{
    public string Name { get; set; } = string.Empty;
    public string Franchise { get; set; } = string.Empty;
    public string[] Characters { get; set; } = [];
    public Guid CategoryId { get; set; }
    public ModelType Type { get; set; }
    public string? Artist { get; set; }
    public string[] Tags { get; set; } = [];
    public BaseSize BaseSize { get; set; } = new();
    public FigureSize FigureSize { get; set; } = new();
    public int NumberOfFigures { get; set; }
}

public class UpdateModelRequest
{
    public string Name { get; set; } = string.Empty;
    public string Franchise { get; set; } = string.Empty;
    public string[] Characters { get; set; } = [];
    public Guid CategoryId { get; set; }
    public ModelType Type { get; set; }
    public string? Artist { get; set; }
    public string[] Tags { get; set; } = [];
    public BaseSize BaseSize { get; set; } = new();
    public FigureSize FigureSize { get; set; } = new();
    public int NumberOfFigures { get; set; }
}

public class CreateModelCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}

public class UpdateModelCategoryRequest
{
    public string Name { get; set; } = string.Empty;
} 