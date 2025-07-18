namespace PaintingProjectsManagement.Features.Materials.Tests;

public class Update_Material_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        //await TestingServer.LoginAsync("superuser", "admin", null);

        using (var context = TestingServer.Context)
        {
            context.Set<Material>().Add(new Material("8x4 magnet", MaterialUnit.Unit, 19));
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// User should be able to update an existing material
    /// </summary>
    [Test, NotInParallel(Order = 2)]
    public async Task User_Can_Update_Material()
    {
        // Prepare - Load the material created in the previous test
        var existingMaterial = TestingServer.Context.Set<Material>().FirstOrDefault(x => x.Name == "8x4 magnet");
        existingMaterial.ShouldNotBeNull("Material should exist from previous test");

        var updateRequest = new UpdateMaterial.Request
        {
            Id = existingMaterial.Id,
            Name = "Updated 8x4 magnet",
            Unit = MaterialUnit.Drops,
            PricePerUnit = 25.50,
        };

        // Act
        var response = await TestingServer.PutAsync("api/materials", updateRequest);

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database
        var updatedEntity = TestingServer.Context.Set<Material>().FirstOrDefault(x => x.Id == existingMaterial.Id);

        updatedEntity.ShouldNotBeNull();
        updatedEntity.Id.ShouldBe(existingMaterial.Id);
        updatedEntity.Name.ShouldBe("Updated 8x4 magnet");
        updatedEntity.PricePerUnit.ShouldBe(25.50);
        updatedEntity.Unit.ShouldBe(MaterialUnit.Drops);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.Context.Database.EnsureDeletedAsync();
    }
}