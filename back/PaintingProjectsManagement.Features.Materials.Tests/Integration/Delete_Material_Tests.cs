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
        // Setup test data directly in database
        using (var context = TestingServer.CreateContext())
        {
            // Create a test tenant
            var tenant = new Tenant("TEST", "Test Tenant", "");
            context.Set<Tenant>().Add(tenant);
            await context.SaveChangesAsync();
        }

        using (var context = TestingServer.CreateContext())
        {
            var connectionString = context.Database.GetConnectionString();

            context.Set<Material>().Add(new Material("TEST", "8x4 magnet", MaterialUnit.Unit, 19));
            await context.SaveChangesAsync();
        }
    }
     
    /// <summary>
    /// User should be able to delete an existing material
    /// </summary>
    [Test, NotInParallel(Order = 2)]
    public async Task User_Can_Delete_Material()
    {
        // Prepare - Load the material created in the previous test
        var existingMaterial = TestingServer.CreateContext().Set<Material>().FirstOrDefault(x => x.Name == "8x4 magnet");
        existingMaterial.ShouldNotBeNull("Material should exist from previous test");

        // Act
        var response = await TestingServer.DeleteAsync($"api/materials/{existingMaterial.Id}");

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