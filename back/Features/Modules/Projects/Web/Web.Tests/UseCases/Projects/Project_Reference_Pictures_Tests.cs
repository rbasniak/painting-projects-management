using Microsoft.AspNetCore.Hosting;
using PaintingProjectsManagement.Infrastructure.Common;

namespace PaintingProjectsManagement.Features.Projects.Tests;

[HumanFriendlyDisplayName]
public class Project_Reference_Pictures_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private readonly string _base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAQSURBVBhXY/iPBkgW+P8fAHg8P8Hpkr/2AAAAAElFTkSuQmCC";
    private static Guid _projectId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        var project = new Project("rodrigo.basniak", "Project With References", DateTime.UtcNow, null);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(project);
            await context.SaveChangesAsync();
        }

        _projectId = project.Id;
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Upload_Project_Reference_Picture_Should_Succeed()
    {
        var response = await TestingServer.PostAsync<UrlReference[]>(
            "api/projects/reference-picture",
            new UploadProjectReferencePicture.Request
            {
                ProjectId = _projectId,
                Base64Image = _base64Image
            }, "rodrigo.basniak");

        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Length.ShouldBe(1);

        var updatedProject = await TestingServer.CreateContext().Set<Project>()
            .Include(x => x.References)
            .FirstAsync(x => x.Id == _projectId);

        updatedProject.References.Count().ShouldBe(1);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Upload_Project_Reference_Picture_Should_Fail_For_Invalid_Image()
    {
        var response = await TestingServer.PostAsync(
            "api/projects/reference-picture",
            new UploadProjectReferencePicture.Request
            {
                ProjectId = _projectId,
                Base64Image = "invalid-base64"
            }, "rodrigo.basniak");

        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Invalid image format.");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Free_Tier_Project_Reference_Picture_Limit_Is_Enforced()
    {
        var second = await TestingServer.PostAsync<UrlReference[]>(
            "api/projects/reference-picture",
            new UploadProjectReferencePicture.Request
            {
                ProjectId = _projectId,
                Base64Image = _base64Image
            }, "rodrigo.basniak");

        var third = await TestingServer.PostAsync<UrlReference[]>(
            "api/projects/reference-picture",
            new UploadProjectReferencePicture.Request
            {
                ProjectId = _projectId,
                Base64Image = _base64Image
            }, "rodrigo.basniak");

        second.ShouldBeSuccess();
        third.ShouldBeSuccess();

        var fourth = await TestingServer.PostAsync(
            "api/projects/reference-picture",
            new UploadProjectReferencePicture.Request
            {
                ProjectId = _projectId,
                Base64Image = _base64Image
            }, "rodrigo.basniak");

        fourth.ShouldHaveErrors(HttpStatusCode.BadRequest, "Project reference picture limit reached for current subscription tier.");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Upload_Project_Reference_Picture_Should_Fail_When_Quota_Is_Exceeded()
    {
        var usageService = TestingServer.Services.GetRequiredService<ITenantStorageUsageService>();
        var env = TestingServer.Services.GetRequiredService<IWebHostEnvironment>();

        var tenant = "RODRIGO.BASNIAK";
        var tenantFolder = Path.Combine(env.WebRootPath, "uploads", tenant, "quota-tests");
        Directory.CreateDirectory(tenantFolder);

        var usedBytes = await usageService.GetUsageInBytesAsync(tenant, CancellationToken.None);
        var fillerBytes = Math.Max(0, usageService.QuotaInBytes - usedBytes);
        var fillerPath = Path.Combine(tenantFolder, $"{Guid.NewGuid():N}.bin");

        await using (var fillerStream = new FileStream(fillerPath, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            fillerStream.SetLength(fillerBytes);
        }

        try
        {
            var response = await TestingServer.PostAsync(
                "api/projects/reference-picture",
                new UploadProjectReferencePicture.Request
                {
                    ProjectId = _projectId,
                    Base64Image = _base64Image
                }, "rodrigo.basniak");

            response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Storage quota exceeded.");
        }
        finally
        {
            File.Delete(fillerPath);
        }
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Promote_Reference_Picture_To_Cover_Should_Update_Project_Picture_Url()
    {
        var upload = await TestingServer.PostAsync<UrlReference[]>(
            "api/projects/reference-picture",
            new UploadProjectReferencePicture.Request
            {
                ProjectId = _projectId,
                Base64Image = _base64Image
            }, "rodrigo.basniak");
        upload.ShouldBeSuccess();
        upload.Data.ShouldNotBeNull();
        upload.Data.Length.ShouldBeGreaterThan(0);

        var pictureUrl = upload.Data.Last().Url;

        var promote = await TestingServer.PostAsync(
            "api/projects/picture/promote",
            new PromoteProjectPictureToCover.Request
            {
                ProjectId = _projectId,
                PictureUrl = pictureUrl
            },
            "rodrigo.basniak");

        promote.ShouldBeSuccess();

        var project = await TestingServer.CreateContext()
            .Set<Project>()
            .FirstAsync(x => x.Id == _projectId);
        project.PictureUrl.ShouldBe(pictureUrl);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
