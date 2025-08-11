using Microsoft.EntityFrameworkCore;

namespace PaintingProjectsManagement.Features.Projects.Tests;

public class Delete_Project_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test projects for different users
        var existingProject = new Project("rodrigo.basniak", "Existing Project", DateTime.UtcNow, null);
        var anotherUserProject = new Project("ricardo.smarzaro", "Another User Project", DateTime.UtcNow, null);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(existingProject);
            await context.AddAsync(anotherUserProject);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedProject = context.Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
            savedProject.ShouldNotBeNull();
            
            var savedAnotherUserProject = context.Set<Project>().FirstOrDefault(x => x.Name == "Another User Project");
            savedAnotherUserProject.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Delete_Project()
    {
        // Prepare - Use a non-existent project ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"api/projects/{nonExistentId}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Delete_Project_That_Does_Not_Exist()
    {
        // Prepare - Use a non-existent project ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"api/projects/{nonExistentId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Delete_Project_That_Belongs_To_Another_User()
    {
        // Prepare - Load the project created by another user
        var anotherUserProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Another User Project");
        anotherUserProject.TenantId.ShouldBe("RICARDO.SMARZARO", "Project should belong to another user");
        anotherUserProject.ShouldNotBeNull("Project should exist from seed");

        // Act - Try to delete as rodrigo.basniak (different user)
        var response = await TestingServer.DeleteAsync($"api/projects/{anotherUserProject.Id}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - project should still exist
        var stillExistingEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == anotherUserProject.Id);
        stillExistingEntity.ShouldNotBeNull("Project should still exist in database");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Can_Delete_Project()
    {
        // Prepare - Load the project created by the same user
        var existingProject = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Name == "Existing Project");
        existingProject.TenantId.ShouldBe("RODRIGO.BASNIAK", "Project should belong to the same user");
        existingProject.ShouldNotBeNull("Project should exist from seed");

        // Act
        var response = await TestingServer.DeleteAsync($"api/projects/{existingProject.Id}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - project should be deleted
        var deletedEntity = TestingServer.CreateContext().Set<Project>().FirstOrDefault(x => x.Id == existingProject.Id);
        deletedEntity.ShouldBeNull("Project should be deleted from database");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 