namespace PaintingProjectsManagement.Features.Projects.Tests;

[HumanFriendlyDisplayName]
public class Get_Execution_Materials_Catalog_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        using (var context = TestingServer.CreateContext())
        {
            await context.Set<Material>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            await context.AddRangeAsync(
                new Material
                {
                    Id = Guid.NewGuid(),
                    Tenant = "RODRIGO.BASNIAK",
                    Name = "AK Black",
                    CategoryId = 40,
                    CategoryName = "Paints",
                    PricePerUnit = new Money(4.50, "USD"),
                    Unit = MaterialUnit.Mililiter,
                    UpdatedUtc = DateTime.UtcNow
                },
                new Material
                {
                    Id = Guid.NewGuid(),
                    Tenant = "RODRIGO.BASNIAK",
                    Name = "3x2 magnet",
                    CategoryId = 20,
                    CategoryName = "Magnets",
                    PricePerUnit = new Money(0.09, "USD"),
                    Unit = MaterialUnit.Unit,
                    UpdatedUtc = DateTime.UtcNow
                },
                new Material
                {
                    Id = Guid.NewGuid(),
                    Tenant = "RODRIGO.BASNIAK",
                    Name = "Vallejo Black",
                    CategoryId = 40,
                    CategoryName = "Paints",
                    PricePerUnit = new Money(5.20, "USD"),
                    Unit = MaterialUnit.Drop,
                    UpdatedUtc = DateTime.UtcNow
                },
                new Material
                {
                    Id = Guid.NewGuid(),
                    Tenant = "RICARDO.SMARZARO",
                    Name = "Ricardo Resin",
                    CategoryId = 10,
                    CategoryName = "Resins",
                    PricePerUnit = new Money(0.03, "USD"),
                    Unit = MaterialUnit.Gram,
                    UpdatedUtc = DateTime.UtcNow
                });

            await context.SaveChangesAsync();
        }

        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Get_Catalog()
    {
        var response = await TestingServer.GetAsync<IReadOnlyCollection<AvailableProjectMaterialDetails>>("projects/execution/materials/available");

        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Authenticated_User_Gets_Only_Their_Materials_Ordered_By_Category_Then_Name()
    {
        var response = await TestingServer.GetAsync<IReadOnlyCollection<AvailableProjectMaterialDetails>>("projects/execution/materials/available", "rodrigo.basniak");

        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(3);

        var items = response.Data.ToList();

        items[0].CategoryName.ShouldBe("Magnets");
        items[0].MaterialName.ShouldBe("3x2 magnet");
        items[0].DefaultUnit.ShouldBe(MaterialUnit.Unit);

        items[1].CategoryName.ShouldBe("Paints");
        items[1].MaterialName.ShouldBe("AK Black");
        items[1].DefaultUnit.ShouldBe(MaterialUnit.Mililiter);

        items[2].CategoryName.ShouldBe("Paints");
        items[2].MaterialName.ShouldBe("Vallejo Black");
        items[2].DefaultUnit.ShouldBe(MaterialUnit.Drop);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_See_Other_Tenant_Materials_In_Catalog()
    {
        var response = await TestingServer.GetAsync<IReadOnlyCollection<AvailableProjectMaterialDetails>>("projects/execution/materials/available", "ricardo.smarzaro");

        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(1);
        response.Data.First().MaterialName.ShouldBe("Ricardo Resin");
        response.Data.First().CategoryName.ShouldBe("Resins");
        response.Data.First().DefaultUnit.ShouldBe(MaterialUnit.Gram);
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Catalog_Returns_Empty_When_User_Has_No_Materials()
    {
        using (var context = TestingServer.CreateContext())
        {
            await context.Set<Material>()
                .Where(x => x.Tenant == "RODRIGO.BASNIAK")
                .ExecuteDeleteAsync();
            await context.SaveChangesAsync();
        }

        var response = await TestingServer.GetAsync<IReadOnlyCollection<AvailableProjectMaterialDetails>>("projects/execution/materials/available", "rodrigo.basniak");

        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
