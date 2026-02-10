namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

public class GetCatalog_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands, lines, and colors for a complete catalog
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
            var line2 = new PaintLine(brand1, "Layer");
            var line3 = new PaintLine(brand2, "Game Color");

            await context.AddAsync(line1);
            await context.AddAsync(line2);
            await context.AddAsync(line3);
            await context.SaveChangesAsync();

            var color1 = new PaintColor(line1, "Abaddon Black", "#000000", 12.0, PaintType.Opaque, "21-25");
            var color2 = new PaintColor(line1, "Corax White", "#FFFFFF", 12.0, PaintType.Opaque, "21-52");
            var color3 = new PaintColor(line2, "Evil Sunz Scarlet", "#FF0000", 12.0, PaintType.Opaque, "22-17");
            var color4 = new PaintColor(line3, "Bloody Red", "#CC0000", 17.0, PaintType.Opaque, "72.010");

            await context.AddAsync(color1);
            await context.AddAsync(color2);
            await context.AddAsync(color3);
            await context.AddAsync(color4);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var brands = context.Set<PaintBrand>().ToList();
            brands.Count.ShouldBe(2);

            var lines = context.Set<PaintLine>().ToList();
            lines.Count.ShouldBe(3);

            var colors = context.Set<PaintColor>().ToList();
            colors.Count.ShouldBe(4);
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Get_Catalog()
    {
        // Act
        var response = await TestingServer.GetAsync<CatalogDetails>("/api/inventory/catalog");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Can_Get_Complete_Catalog()
    {
        // Act
        var response = await TestingServer.GetAsync<CatalogDetails>("/api/inventory/catalog", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();

        // Verify catalog structure
        response.Data.Brands.Count.ShouldBe(2);

        // Verify first brand (Citadel)
        var citadel = response.Data.Brands.FirstOrDefault(x => x.Name == "Citadel");
        citadel.ShouldNotBeNull();
        citadel.Lines.Count.ShouldBe(2);

        // Verify first brand's lines
        var baseLine = citadel.Lines.FirstOrDefault(x => x.Name == "Base");
        baseLine.ShouldNotBeNull();
        baseLine.Paints.Count.ShouldBe(2);

        var layerLine = citadel.Lines.FirstOrDefault(x => x.Name == "Layer");
        layerLine.ShouldNotBeNull();
        layerLine.Paints.Count.ShouldBe(1);

        // Verify second brand (Vallejo)
        var vallejo = response.Data.Brands.FirstOrDefault(x => x.Name == "Vallejo");
        vallejo.ShouldNotBeNull();
        vallejo.Lines.Count.ShouldBe(1);

        var gameColorLine = vallejo.Lines.FirstOrDefault(x => x.Name == "Game Color");
        gameColorLine.ShouldNotBeNull();
        gameColorLine.Paints.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Catalog_Contains_Correct_Paint_Details()
    {
        // Act
        var response = await TestingServer.GetAsync<CatalogDetails>("/api/inventory/catalog", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();

        // Find a specific paint and verify its details
        var citadel = response.Data.Brands.FirstOrDefault(x => x.Name == "Citadel");
        citadel.ShouldNotBeNull();

        var baseLine = citadel.Lines.FirstOrDefault(x => x.Name == "Base");
        baseLine.ShouldNotBeNull();

        var abaddonBlack = baseLine.Paints.FirstOrDefault(x => x.Name == "Abaddon Black");
        abaddonBlack.ShouldNotBeNull();
        abaddonBlack.HexColor.ShouldBe("#000000");
        abaddonBlack.BottleSize.ShouldBe(12.0);
        abaddonBlack.Type.ShouldBe(PaintType.Opaque);
        abaddonBlack.ManufacturerCode.ShouldBe("21-25");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Empty_Catalog_Returns_Empty_Structure()
    {
        // Prepare - Remove all data
        using (var context = TestingServer.CreateContext())
        {
            await context.Set<PaintColor>().ExecuteDeleteAsync();
            await context.Set<PaintLine>().ExecuteDeleteAsync();
            await context.Set<PaintBrand>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();
        }

        // Act
        var response = await TestingServer.GetAsync<CatalogDetails>("/api/inventory/catalog", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Brands.Count.ShouldBe(0);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
