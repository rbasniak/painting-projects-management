using Microsoft.AspNetCore.Hosting;
using PaintingProjectsManagement.Features.Authorization;
using PaintingProjectsManagement.Infrastructure.Common;

namespace PaintingProjectsManagement.Features.Models.Tests;

[HumanFriendlyDisplayName]
public class Profile_Endpoints_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static string? _probeFilePath;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Get_Profile_Details()
    {
        var response = await TestingServer.GetAsync<ProfileDetails>("api/profile/me");
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Authenticated_User_Can_Get_Profile_Details()
    {
        var response = await TestingServer.GetAsync<ProfileDetails>("api/profile/me", "rodrigo.basniak");

        response.ShouldBeSuccess(out var profile);
        profile.Username.ShouldBe("rodrigo.basniak");
        profile.Tenant.ShouldBe("RODRIGO.BASNIAK");
        profile.Email.ShouldNotBeNullOrWhiteSpace();
        profile.DisplayName.ShouldNotBeNullOrWhiteSpace();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Non_Authenticated_User_Cannot_Get_Storage_Usage()
    {
        var response = await TestingServer.GetAsync<StorageUsageDetails>("api/profile/storage-usage");
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Storage_Usage_Reflects_Disk_Consumption_For_User_Files()
    {
        var baselineResponse = await TestingServer.GetAsync<StorageUsageDetails>("api/profile/storage-usage", "rodrigo.basniak");
        baselineResponse.ShouldBeSuccess(out var baseline);

        var env = TestingServer.Services.GetRequiredService<IWebHostEnvironment>();
        var tenantFolder = Path.Combine(env.WebRootPath, "uploads", "RODRIGO.BASNIAK", "profile-tests");
        Directory.CreateDirectory(tenantFolder);

        var expectedBytes = 4096;
        _probeFilePath = Path.Combine(tenantFolder, $"{Guid.NewGuid():N}.bin");
        await using (var fileStream = new FileStream(_probeFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            fileStream.SetLength(expectedBytes);
        }

        var updatedResponse = await TestingServer.GetAsync<StorageUsageDetails>("api/profile/storage-usage", "rodrigo.basniak");
        updatedResponse.ShouldBeSuccess(out var updated);

        updated.QuotaBytes.ShouldBe(StorageQuotaOptions.DefaultQuotaInBytes);
        (updated.UsedBytes - baseline.UsedBytes).ShouldBeGreaterThanOrEqualTo(expectedBytes);
        updated.RemainingBytes.ShouldBe(updated.QuotaBytes - updated.UsedBytes);

        File.Delete(_probeFilePath);
        _probeFilePath = null;
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        if (!string.IsNullOrWhiteSpace(_probeFilePath) && File.Exists(_probeFilePath))
        {
            File.Delete(_probeFilePath);
        }

        await TestingServer.DisposeAsync();
    }
}
