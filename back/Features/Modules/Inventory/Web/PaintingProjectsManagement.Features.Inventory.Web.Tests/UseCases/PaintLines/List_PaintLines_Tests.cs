namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

[HumanFriendlyDisplayName]
public class List_PaintLines_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands and lines
        var brand1 = new PaintBrand("Citadel");
        var brand2 = new PaintBrand("Vallejo");

        using (var context = TestingServer.CreateContext())
        {
            await context.Set<PaintLine>().ExecuteDeleteAsync();
            await context.Set<PaintBrand>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            await context.AddAsync(brand1);
            await context.AddAsync(brand2);
            await context.SaveChangesAsync();

            var line1 = new PaintLine(brand1, "Base");
            var line2 = new PaintLine(brand1, "Layer");
            var line3 = new PaintLine(brand2, "Game Color");

            await context.AddAsync(line1);
            await context.AddAsync(line2);
            await context.AddAsync(line3);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var lines = context.Set<PaintLine>().ToList();
            lines.Count.ShouldBe(3);
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_Paint_Lines()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintLineDetails>>("/api/paints/lines");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Can_List_Paint_Lines()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintLineDetails>>("/api/paints/lines", "superuser");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(3);

        // Verify lines are returned correctly
        var lineNames = response.Data.Select(x => x.Name).ToList();
        lineNames.ShouldContain("Base");
        lineNames.ShouldContain("Layer");
        lineNames.ShouldContain("Game Color");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Lines_Are_Ordered_By_Brand_Then_Name()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintLineDetails>>("/api/paints/lines", "superuser");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();

        // Verify ordering (by brand name, then by line name)
        var lines = response.Data.ToList();
        
        // First two should be from Citadel (alphabetically first brand)
        lines[0].Brand.Name.ShouldBe("Citadel");
        lines[0].Name.ShouldBe("Base");
        lines[1].Brand.Name.ShouldBe("Citadel");
        lines[1].Name.ShouldBe("Layer");
        
        // Third should be from Vallejo
        lines[2].Brand.Name.ShouldBe("Vallejo");
        lines[2].Name.ShouldBe("Game Color");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
