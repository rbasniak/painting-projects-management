using PaintingProjectsManagement.UI.Client.Modules.Materials.Models;

namespace PaintingProjectsManagement.UI.Client.Modules.Materials.Services;

public interface IMaterialsService
{
    Task<IReadOnlyCollection<MaterialDetails>> GetMaterialsAsync();
    Task<MaterialDetails> CreateMaterialAsync(CreateMaterialRequest request);
    Task<MaterialDetails> UpdateMaterialAsync(UpdateMaterialRequest request);
    Task DeleteMaterialAsync(Guid id);
} 