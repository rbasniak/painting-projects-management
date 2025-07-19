using Microsoft.EntityFrameworkCore;
using rbkApiModules.Identity.Core;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class Delete_Material_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test materials for different users
        var existingMaterial = new Material("rodrigo.basniak", "Existing Material", MaterialUnit.Unit, 10.0);
        var anotherUserMaterial = new Material("ricardo.smarzaro", "Another User Material", MaterialUnit.Unit, 5.0);

        using (var context = TestingServer.CreateContext())
        {
            var connectionString = context.Database.GetConnectionString();

            await context.AddAsync(existingMaterial);
            await context.AddAsync(anotherUserMaterial);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedMaterial = context.Set<Material>().FirstOrDefault(x => x.Name == "Existing Material");
            savedMaterial.ShouldNotBeNull();
            
            var savedAnotherUserMaterial = context.Set<Material>().FirstOrDefault(x => x.Name == "Another User Material");
            savedAnotherUserMaterial.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Delete_Material()
    {
        // Prepare - Use a non-existent material ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"api/materials/{nonExistentId}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// User should not be able to delete a material that does not exist
    /// </summary>
    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Delete_Material_That_Does_Not_Exist()
    {
        // Prepare - Use a non-existent material ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"api/materials/{nonExistentId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");
    }

    /// <summary>
    /// User should not be able to delete a material that belongs to another user
    /// </summary>
    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Delete_Material_That_Belongs_To_Another_User()
    {
        // Prepare - Load the material created by another user
        var anotherUserMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Name == "Another User Material");
        anotherUserMaterial.TenantId.ShouldBe("RICARDO.SMARZARO", "Material should belong to another user");
        anotherUserMaterial.ShouldNotBeNull("Material should exist from seed");

        // Act - Try to delete as rodrigo.basniak (different user)
        var response = await TestingServer.DeleteAsync($"api/materials/{anotherUserMaterial.Id}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - material should still exist
        var stillExistingEntity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == anotherUserMaterial.Id);
        stillExistingEntity.ShouldNotBeNull("Material should still exist in database");
    }

    /// <summary>
    /// User should not be able to delete a material that is used in any project
    /// </summary>
    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Delete_Material_Used_In_Project()
    {
        // TODO: Implement test for material used in project validation
        // This test should verify that a material cannot be deleted if it's referenced in any project
        // The implementation will depend on the project-material relationship structure
    }
     
    /// <summary>
    /// User should be able to delete an existing material
    /// </summary>
    [Test, NotInParallel(Order = 6)]
    public async Task User_Can_Delete_Material()
    {
        // Prepare - Load the material created in the seed
        var existingMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Name == "Existing Material");
        existingMaterial.ShouldNotBeNull("Material should exist from seed");

        // Act
        var response = await TestingServer.DeleteAsync($"api/materials/{existingMaterial.Id}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - material should be deleted
        var deletedEntity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == existingMaterial.Id);
        deletedEntity.ShouldBeNull("Material should be deleted from database");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
}