namespace PaintingProjectsManagement.Features.Materials.Tests;

public class Create_Material_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        //await TestingServer.LoginAsync("superuser", "admin", null);
    }

    /// <summary>
    /// The global admin should be able to create a new application claim
    /// </summary>
    [Test, NotInParallel(Order = 2)]
    public async Task User_Can_Create_Material()
    {
        // Prepare
        var request = new CreateMaterial.Request
        {
            Name = "8x4 magnet",
            Unit = MaterialUnit.Unit,
            PricePerUnit = 19,
        };

        // Act
        var response = await TestingServer.PostAsync<MaterialDetails>("api/materials", request);

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("8x4 magnet");
        result.PricePerUnit.ShouldBe(19);
        result.Unit.ShouldNotBeNull();
        result.Unit.Id.ShouldBe((int)MaterialUnit.Unit);
        result.Unit.Value.ShouldBe(MaterialUnit.Unit.ToString());


        // Assert the database
        var entity = TestingServer.Context.Set<Material>().FirstOrDefault(x => x.Id == result.Id);

        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(result.Id);
        entity.Name.ShouldBe("8x4 magnet");
        entity.PricePerUnit.ShouldBe(19);
        entity.Unit.ShouldBe(MaterialUnit.Unit);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.Context.Database.EnsureDeletedAsync();
    }
}