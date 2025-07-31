using PaintingProjectsManagement.Blazor.Features.Materials.Models;

namespace PaintingProjectsManagement.Blazor.Features.Materials.Services;

public interface IMaterialsApiService
{
    Task<List<Material>> GetMaterialsAsync();
    Task<Material?> GetMaterialAsync(Guid id);
    Task<Material> CreateMaterialAsync(CreateMaterialRequest request);
    Task<Material> UpdateMaterialAsync(Guid id, UpdateMaterialRequest request);
    Task DeleteMaterialAsync(Guid id);
}

public class CreateMaterialRequest
{
    public string Name { get; set; } = string.Empty;
    public MaterialUnit Unit { get; set; } = new();
    public double PricePerUnit { get; set; }
}

public class UpdateMaterialRequest
{
    public string Name { get; set; } = string.Empty;
    public MaterialUnit Unit { get; set; } = new();
    public double PricePerUnit { get; set; }
} 