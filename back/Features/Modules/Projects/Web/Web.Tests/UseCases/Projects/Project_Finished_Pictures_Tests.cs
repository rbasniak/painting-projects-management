using System.Net;
using PaintingProjectsManagement.Infrastructure.Common;

namespace PaintingProjectsManagement.Features.Projects.Tests;

[HumanFriendlyDisplayName]
public class Project_Finished_Pictures_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private readonly string _base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAQSURBVBhXY/iPBkgW+P8fAHg8P8Hpkr/2AAAAAElFTkSuQmCC";
    private static Guid _projectId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        var project = new Project("rodrigo.basniak", "Project With Finished Pictures", DateTime.UtcNow, null);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(project);
            await context.SaveChangesAsync();
        }

        _projectId = project.Id;
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Free_Tier_Finished_Project_Picture_Limit_Is_Enforced()
    {
        for (var i = 0; i < 3; i++)
        {
            var upload = await TestingServer.PostAsync<UrlReference[]>(
                "api/projects/finished-picture",
                new UploadProjectFinishedPicture.Request
                {
                    ProjectId = _projectId,
                    Base64Image = _base64Image
                }, "rodrigo.basniak");

            upload.ShouldBeSuccess();
        }

        var fourth = await TestingServer.PostAsync(
            "api/projects/finished-picture",
            new UploadProjectFinishedPicture.Request
            {
                ProjectId = _projectId,
                Base64Image = _base64Image
            }, "rodrigo.basniak");

        fourth.ShouldHaveErrors(HttpStatusCode.BadRequest, "Project finished picture limit reached for current subscription tier.");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
