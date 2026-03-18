namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

[HumanFriendlyDisplayName]
public class List_PaintBrands_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands
        var brand1 = new PaintBrand("Citadel");
        var brand2 = new PaintBrand("Vallejo");
        var brand3 = new PaintBrand("Army Painter");

        using (var context = TestingServer.CreateContext())
        {
            await context.Set<PaintBrand>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            await context.AddAsync(brand1);
            await context.AddAsync(brand2);
            await context.AddAsync(brand3);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var brands = context.Set<PaintBrand>().ToList();
            brands.Count.ShouldBe(3);
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_Paint_Brands()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintBrandDetails>>("/api/paints/brands");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Can_List_Paint_Brands()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintBrandDetails>>("/api/paints/brands", "superuser");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(3);

        // Verify brands are returned correctly
        var brandNames = response.Data.Select(x => x.Name).ToList();
        brandNames.ShouldContain("Citadel");
        brandNames.ShouldContain("Vallejo");
        brandNames.ShouldContain("Army Painter");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Brands_Are_Ordered_Alphabetically()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintBrandDetails>>("/api/paints/brands", "superuser");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();

        // Verify ordering (alphabetically)
        var brands = response.Data.ToList();
        brands[0].Name.ShouldBe("Army Painter");
        brands[1].Name.ShouldBe("Citadel");
        brands[2].Name.ShouldBe("Vallejo");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
