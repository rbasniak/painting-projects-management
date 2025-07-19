using Microsoft.EntityFrameworkCore;

namespace PaintingProjectsManagement.Features.Materials.Tests;

public class Update_Material_Tests
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

    /// <summary>
    /// User should be able to update an existing material
    /// </summary>
    [Test, NotInParallel(Order = 2)]
    public async Task User_Can_Update_Material()
    {
        // Prepare - Load the material created in the seed test
        var existingMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Name == "Existing Material");
        existingMaterial.ShouldNotBeNull("Material should exist from seed");

        var updateRequest = new UpdateMaterial.Request
        {
            Id = existingMaterial.Id,
            Name = "Updated 8x4 magnet",
            Unit = MaterialUnit.Drops,
            PricePerUnit = 25.50,
        };

        // Act
        var response = await TestingServer.PutAsync("api/materials", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database
        var updatedEntity = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Id == existingMaterial.Id);

        updatedEntity.ShouldNotBeNull();
        updatedEntity.Id.ShouldBe(existingMaterial.Id);
        updatedEntity.Name.ShouldBe("Updated 8x4 magnet");
        updatedEntity.PricePerUnit.ShouldBe(25.50);
        updatedEntity.Unit.ShouldBe(MaterialUnit.Drops);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
}