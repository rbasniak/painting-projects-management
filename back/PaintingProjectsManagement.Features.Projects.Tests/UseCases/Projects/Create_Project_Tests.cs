namespace PaintingProjectsManagement.Features.Projects.Tests;

public class Create_Project_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create a test project for duplicate name validation tests
        var existingProject = new Project("rodrigo.basniak", "Existing Project", DateTime.UtcNow, null);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(existingProject);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedProject = context.Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
            savedProject.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_Project()
    {
        // Prepare
        var request = new CreateProject.Request
        {
            Name = "Test Project",
            // Base64Image = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAgAAAAIAQMAAAD+wSzIAAAABlBMVEX///+/v7+jQ3Y5AAAADklEQVQI12P4AIX8EAgALgAD/aNpbtEAAAAASUVORK5CYII=",
        };

        // Act
        var response = await TestingServer.PostAsync<ProjectHeader>("api/projects", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var projects = TestingServer.CreateContext().Set<Project>().Where(x => x.Name == "Test Project").ToList();
        projects.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 3)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Create_Project_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var request = new CreateProject.Request
        {
            Name = name!,
        };

        // Act
        var response = await TestingServer.PostAsync<ProjectHeader>("api/projects", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var projects = TestingServer.CreateContext().Set<Project>().Where(x => x.Name == name).ToList();
        projects.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Create_Project_When_Name_Exceeds_MaxLength()
    {
        // Prepare
        var request = new CreateProject.Request
        {
            Name = new string('A', 101), // Exceeds max length of 100
        };

        // Act
        var response = await TestingServer.PostAsync<ProjectHeader>("api/projects", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database
        var projects = TestingServer.CreateContext().Set<Project>().Where(x => x.Name == new string('A', 101)).ToList();
        projects.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Create_Project_When_Name_Already_Exists()
    {
        // Prepare
        var request = new CreateProject.Request
        {
            Name = "Existing Project", // This name was created by rodrigo.basniak in Seed test
        };

        // Act
        var response = await TestingServer.PostAsync<ProjectHeader>("api/projects", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A project with this name already exists.");

        // Assert the database - should still have only one project with this name
        var projects = TestingServer.CreateContext().Set<Project>().Where(x => x.Name == "Existing Project" && x.TenantId == "RODRIGO.BASNIAK").ToList();
        projects.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 11)]
    public async Task User_Can_Create_Project_With_Same_Name_As_Another_User()
    {
        // Prepare
        var request = new CreateProject.Request
        {
            Name = "Existing Project", // This name was created by rodrigo.basniak in Seed test
        };

        // Act
        var response = await TestingServer.PostAsync<ProjectHeader>("api/projects", request, "ricardo.smarzaro");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Existing Project");
        result.PictureUrl.ShouldNotBeEmpty();

        // Assert the database - should have two projects with the same name but different users
        var projects = TestingServer.CreateContext().Set<Project>().Where(x => x.Name == "Existing Project").ToList();
        projects.Count.ShouldBe(2); // One from rodrigo.basniak (Seed) and one from ricardo.smarzaro

        var rbProject = projects.FirstOrDefault(x => x.TenantId == "RODRIGO.BASNIAK" && x.Name == "Existing Project");
        var rsProject = projects.FirstOrDefault(x => x.TenantId == "RICARDO.SMARZARO" && x.Name == "Existing Project");

        rbProject.ShouldNotBeNull();
        rbProject.Id.ShouldNotBe(rsProject.Id);
    }

    [Test, NotInParallel(Order = 12)]
    public async Task User_Can_Create_Project()
    {
        // Prepare
        var request = new CreateProject.Request
        {
            Name = "Test Project",
        };

        // Act
        var response = await TestingServer.PostAsync<ProjectHeader>("api/projects", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Test Project");
        result.PictureUrl.ShouldNotBeEmpty();

        // Assert the database
        var entity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == result.Id);

        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(result.Id);
        entity.Name.ShouldBe("Test Project");
        entity.PictureUrl.ShouldNotBeEmpty();
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 