namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

public class List_PaintColors_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands, lines, and colors
        var brand1 = new PaintBrand("Citadel");
        var brand2 = new PaintBrand("Vallejo");

        using (var context = TestingServer.CreateContext())
        {
            await context.Set<PaintColor>().ExecuteDeleteAsync();
            await context.Set<PaintLine>().ExecuteDeleteAsync();
            await context.Set<PaintBrand>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            await context.AddAsync(brand1);
            await context.AddAsync(brand2);
            await context.SaveChangesAsync();

            var line1 = new PaintLine(brand1, "Base");
            var line2 = new PaintLine(brand2, "Game Color");

            await context.AddAsync(line1);
            await context.AddAsync(line2);
            await context.SaveChangesAsync();

            var color1 = new PaintColor(line1, "Abaddon Black", "#000000", 12.0, PaintType.Opaque, "21-25");
            var color2 = new PaintColor(line1, "Corax White", "#FFFFFF", 12.0, PaintType.Opaque, "21-52");
            var color3 = new PaintColor(line2, "Bloody Red", "#FF0000", 17.0, PaintType.Opaque, "72.010");

            await context.AddAsync(color1);
            await context.AddAsync(color2);
            await context.AddAsync(color3);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var colors = context.Set<PaintColor>().ToList();
            colors.Count.ShouldBe(3);
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_Paint_Colors()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintColorDetails>>("/api/paints/colors");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Can_List_Paint_Colors()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintColorDetails>>("/api/paints/colors", "superuser");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(3);

        // Verify colors are returned correctly
        var colorNames = response.Data.Select(x => x.Name).ToList();
        colorNames.ShouldContain("Abaddon Black");
        colorNames.ShouldContain("Corax White");
        colorNames.ShouldContain("Bloody Red");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Colors_Are_Ordered_By_Brand_Then_Line_Then_Name()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintColorDetails>>("/api/paints/colors", "superuser");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();

        // Verify ordering (by brand name, then line name, then color name)
        var colors = response.Data.ToList();
        
        // First two should be from Citadel > Base
        colors[0].Line.Name.ShouldBe("Base");
        colors[0].Name.ShouldBe("Abaddon Black");
        colors[1].Line.Name.ShouldBe("Base");
        colors[1].Name.ShouldBe("Corax White");
        
        // Third should be from Vallejo > Game Color
        colors[2].Line.Name.ShouldBe("Game Color");
        colors[2].Name.ShouldBe("Bloody Red");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
