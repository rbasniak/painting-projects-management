namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

[HumanFriendlyDisplayName]
public class Update_PaintColor_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brand, line, and colors
        var brand = new PaintBrand("Test Brand");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(brand);
            await context.SaveChangesAsync();

            var line = new PaintLine(brand, "Test Line");
            await context.AddAsync(line);
            await context.SaveChangesAsync();

            var existingColor = new PaintColor(line, "Existing Color", "#FF0000", 17.0, PaintType.Opaque, "EC-01");
            var duplicateNameColor = new PaintColor(line, "Duplicate Name Color", "#00FF00", 17.0, PaintType.Opaque, "DNC-01");

            await context.AddAsync(existingColor);
            await context.AddAsync(duplicateNameColor);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedColor = context.Set<PaintColor>().FirstOrDefault(x => x.Name == "Existing Color");
            savedColor.ShouldNotBeNull();

            var savedDuplicateColor = context.Set<PaintColor>().FirstOrDefault(x => x.Name == "Duplicate Name Color");
            savedDuplicateColor.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Update_Paint_Color()
    {
        // Prepare
        var existingColor = TestingServer.CreateContext().Set<PaintColor>().Include(x => x.Line).FirstOrDefault(x => x.Name == "Existing Color");
        existingColor.ShouldNotBeNull("Color should exist from seed");

        var updateRequest = new UpdatePaintColor.Request
        {
            Id = existingColor.Id,
            Name = "Updated Color",
            HexColor = "#0000FF",
            BottleSize = 17.0,
            Type = PaintType.Metallic,
            LineId = existingColor.LineId,
            ManufacturerCode = "UC-01"
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/colors", updateRequest);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == existingColor.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Color");
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Update_Paint_Color_That_Does_Not_Exist()
    {
        // Prepare
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Name == "Test Line");
        line.ShouldNotBeNull();

        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdatePaintColor.Request
        {
            Id = nonExistentId,
            Name = "Updated Color",
            HexColor = "#0000FF",
            BottleSize = 17.0,
            Type = PaintType.Metallic,
            LineId = line.Id,
            ManufacturerCode = "UC-01"
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/colors", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Updated Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Update_Paint_Color_When_Name_Already_Exists_In_Same_Line()
    {
        // Prepare
        var existingColor = TestingServer.CreateContext().Set<PaintColor>().Include(x => x.Line).FirstOrDefault(x => x.Name == "Existing Color");
        existingColor.ShouldNotBeNull("Color should exist from seed");

        var updateRequest = new UpdatePaintColor.Request
        {
            Id = existingColor.Id,
            Name = "Duplicate Name Color", // This name already exists in same line
            HexColor = "#0000FF",
            BottleSize = 17.0,
            Type = PaintType.Metallic,
            LineId = existingColor.LineId,
            ManufacturerCode = "UC-01"
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/colors", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Another paint color with this name already exists in this paint line.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == existingColor.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Color");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Update_Paint_Color_When_HexColor_Already_Exists_In_Same_Line()
    {
        // Prepare
        var existingColor = TestingServer.CreateContext().Set<PaintColor>().Include(x => x.Line).FirstOrDefault(x => x.Name == "Existing Color");
        existingColor.ShouldNotBeNull("Color should exist from seed");

        var updateRequest = new UpdatePaintColor.Request
        {
            Id = existingColor.Id,
            Name = "Updated Color",
            HexColor = "#00FF00", // This hex color already exists in same line
            BottleSize = 17.0,
            Type = PaintType.Metallic,
            LineId = existingColor.LineId,
            ManufacturerCode = "UC-01"
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/colors", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Another paint color with this hex color already exists in this paint line.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == existingColor.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.HexColor.ShouldBe("#FF0000");
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Update_Paint_Color_When_HexColor_Is_Invalid()
    {
        // Prepare
        var existingColor = TestingServer.CreateContext().Set<PaintColor>().Include(x => x.Line).FirstOrDefault(x => x.Name == "Existing Color");
        existingColor.ShouldNotBeNull("Color should exist from seed");

        var updateRequest = new UpdatePaintColor.Request
        {
            Id = existingColor.Id,
            Name = "Updated Color",
            HexColor = "invalid", // Invalid hex color format
            BottleSize = 17.0,
            Type = PaintType.Metallic,
            LineId = existingColor.LineId,
            ManufacturerCode = "UC-01"
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/colors", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Hex color must be in format #RRGGBB");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == existingColor.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.HexColor.ShouldBe("#FF0000");
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Can_Update_Paint_Color()
    {
        // Prepare
        var existingColor = TestingServer.CreateContext().Set<PaintColor>().Include(x => x.Line).ThenInclude(x => x.Brand).FirstOrDefault(x => x.Name == "Existing Color");
        existingColor.ShouldNotBeNull("Color should exist from seed");

        var updateRequest = new UpdatePaintColor.Request
        {
            Id = existingColor.Id,
            Name = "Updated Color",
            HexColor = "#0000FF",
            BottleSize = 20.0,
            Type = PaintType.Metallic,
            LineId = existingColor.LineId,
            ManufacturerCode = "UC-01"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("/api/paints/colors", updateRequest, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingColor.Id);
        result.Name.ShouldBe("Updated Color");
        result.HexColor.ShouldBe("#0000FF");
        result.BottleSize.ShouldBe(20.0);
        result.Type.ShouldBe(PaintType.Metallic);
        result.ManufacturerCode.ShouldBe("UC-01");

        // Assert the database
        var updatedEntity = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == existingColor.Id);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("Updated Color");
        updatedEntity.HexColor.ShouldBe("#0000FF");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
