using PaintingProjectsManagement.Infrastructure.Common;
using rbkApiModules.Core.Utilities;

namespace PaintingProjectsManagement.Features.Models;

public interface IModelPictureEntitlementProvider
{
    Task<int?> GetMaxModelPicturesPerModelAsync(string tenantId, CancellationToken cancellationToken);
}

public interface IModelPictureUploadPolicyService
{
    bool HasValidImageExtension(string base64Image);
    Task<bool> HasAvailableQuotaAsync(string tenantId, string base64Image, CancellationToken cancellationToken);
    Task<bool> HasAvailableModelPicturesLimitAsync(string tenantId, int currentPictures, CancellationToken cancellationToken);
}

public sealed class ModelPictureUploadPolicyService(
    ITenantStorageUsageService storageUsageService,
    IModelPictureEntitlementProvider entitlementProvider) : IModelPictureUploadPolicyService
{
    public bool HasValidImageExtension(string base64Image)
    {
        try
        {
            var extension = ImageUtilities.ExtractExtension(base64Image);
            return extension.Equals("jpg", StringComparison.InvariantCultureIgnoreCase) ||
                   extension.Equals("png", StringComparison.InvariantCultureIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> HasAvailableQuotaAsync(string tenantId, string base64Image, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(base64Image) || !HasValidImageExtension(base64Image))
        {
            return true;
        }

        return await storageUsageService.HasQuotaForImageAsync(
            tenantId,
            base64Image,
            bytesToRelease: 0,
            cancellationToken);
    }

    public async Task<bool> HasAvailableModelPicturesLimitAsync(string tenantId, int currentPictures, CancellationToken cancellationToken)
    {
        var maxPictures = await entitlementProvider.GetMaxModelPicturesPerModelAsync(tenantId, cancellationToken);
        if (!maxPictures.HasValue)
        {
            return true;
        }

        if (maxPictures.Value == int.MaxValue)
        {
            return true;
        }

        return currentPictures < maxPictures.Value;
    }
}

public interface IModelPictureStorageService
{
    Task<string> StorePictureAsync(Guid modelId, string tenantId, string base64Image, CancellationToken cancellationToken);
}

public sealed class ModelPictureStorageService(IFileStorage fileStorage) : IModelPictureStorageService
{
    private const int MaxWidth = 2048;
    private const int MaxHeight = 2048;

    public async Task<string> StorePictureAsync(Guid modelId, string tenantId, string base64Image, CancellationToken cancellationToken)
    {
        var baseFileName = $"model_{modelId:N}_{DateTime.UtcNow.Ticks}";
        var fullFileName = $"{baseFileName}.{ImageUtilities.ExtractExtension(base64Image)}";

        return await fileStorage.StoreFileFromBase64Async(
            base64Image,
            fullFileName,
            Path.Combine(tenantId, "models"),
            MaxWidth,
            MaxHeight,
            cancellationToken);
    }
}
