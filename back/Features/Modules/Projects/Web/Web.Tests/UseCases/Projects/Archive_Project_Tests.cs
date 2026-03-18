namespace PaintingProjectsManagement.Features.Projects.Tests;

[HumanFriendlyDisplayName]
public class Archive_Project_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        var activeProject = new Project("rodrigo.basniak", "Active Project", DateTime.UtcNow, null);
        var alreadyArchivedProject = new Project("rodrigo.basniak", "Already Archived Project", DateTime.UtcNow, null);
        alreadyArchivedProject.Archive(DateTime.UtcNow.AddDays(-2));

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(activeProject);
            await context.AddAsync(alreadyArchivedProject);
            await context.SaveChangesAsync();
        }

        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Archive_Project()
    {
        var project = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Active Project");
        project.ShouldNotBeNull();

        var response = await TestingServer.PostAsync<ProjectHeader>($"api/projects/{project.Id}/archive", new { });
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Archive_Project_That_Does_Not_Exist()
    {
        var response = await TestingServer.PostAsync<ProjectHeader>($"api/projects/{Guid.NewGuid()}/archive", new { }, "rodrigo.basniak");
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Can_Archive_Project()
    {
        var project = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Active Project");
        project.ShouldNotBeNull();
        project.EndDate.ShouldBeNull();

        var response = await TestingServer.PostAsync<ProjectHeader>($"api/projects/{project.Id}/archive", new { }, "rodrigo.basniak");

        response.ShouldBeSuccess(out var result);
        result.IsArchived.ShouldBeTrue();
        result.EndDate.ShouldNotBeNull();

        var saved = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == project.Id);
        saved.ShouldNotBeNull();
        saved.EndDate.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Archive_Project_That_Is_Already_Archived()
    {
        var project = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Already Archived Project");
        project.ShouldNotBeNull();
        project.EndDate.ShouldNotBeNull();

        var response = await TestingServer.PostAsync<ProjectHeader>($"api/projects/{project.Id}/archive", new { }, "rodrigo.basniak");
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Project is already archived.");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
