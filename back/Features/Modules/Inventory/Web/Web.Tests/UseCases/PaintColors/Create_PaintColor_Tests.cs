namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

[HumanFriendlyDisplayName]
public class Create_PaintColor_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brand, line, and existing color
        var brand = new PaintBrand("Test Brand");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(brand);
            await context.SaveChangesAsync();

            var line = new PaintLine(brand, "Test Line");
            await context.AddAsync(line);
            await context.SaveChangesAsync();

            var existingColor = new PaintColor(line, "Existing Color", "#FF0000", 17.0, PaintType.Opaque, "EC-01");
            await context.AddAsync(existingColor);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedColor = context.Set<PaintColor>().FirstOrDefault(x => x.Name == "Existing Color");
            savedColor.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_Paint_Color()
    {
        // Prepare
        var line = TestingServer.CreateContext().Set<PaintLine>().Include(x => x.Brand).FirstOrDefault(x => x.Name == "Test Line");
        line.ShouldNotBeNull();

        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "#00FF00",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = line.Id,
            ManufacturerCode = "TC-01"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("/api/paints/colors", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 3)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Create_Paint_Color_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var line = TestingServer.CreateContext().Set<PaintLine>().Include(x => x.Brand).FirstOrDefault(x => x.Name == "Test Line");
        line.ShouldNotBeNull();

        var request = new CreatePaintColor.Request
        {
            Name = name!,
            HexColor = "#00FF00",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = line.Id,
            ManufacturerCode = "TC-01"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("/api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == name).ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Create_Paint_Color_When_Name_Exceeds_MaxLength()
    {
        // Prepare
        var line = TestingServer.CreateContext().Set<PaintLine>().Include(x => x.Brand).FirstOrDefault(x => x.Name == "Test Line");
        line.ShouldNotBeNull();

        var request = new CreatePaintColor.Request
        {
            Name = new string('A', 101), // Exceeds max length of 100
            HexColor = "#00FF00",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = line.Id,
            ManufacturerCode = "TC-01"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("/api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == new string('A', 101)).ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Create_Paint_Color_When_LineId_Does_Not_Exist()
    {
        // Prepare
        var nonExistentLineId = Guid.NewGuid();

        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "#00FF00",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = nonExistentLineId,
            ManufacturerCode = "TC-01"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("/api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "LineId references a non-existent record.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Create_Paint_Color_When_HexColor_Is_Invalid()
    {
        // Prepare
        var line = TestingServer.CreateContext().Set<PaintLine>().Include(x => x.Brand).FirstOrDefault(x => x.Name == "Test Line");
        line.ShouldNotBeNull();

        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "invalid", // Invalid hex color format
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = line.Id,
            ManufacturerCode = "TC-01"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("/api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Hex color must be in format #RRGGBB");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Cannot_Create_Paint_Color_When_BottleSize_Is_Zero_Or_Negative()
    {
        // Prepare
        var line = TestingServer.CreateContext().Set<PaintLine>().Include(x => x.Brand).FirstOrDefault(x => x.Name == "Test Line");
        line.ShouldNotBeNull();

        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "#00FF00",
            BottleSize = 0, // Invalid bottle size
            Type = PaintType.Opaque,
            LineId = line.Id,
            ManufacturerCode = "TC-01"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("/api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Bottle size must be greater than zero.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 8)]
    public async Task User_Cannot_Create_Paint_Color_When_Name_Already_Exists_In_Same_Line()
    {
        // Prepare
        var line = TestingServer.CreateContext().Set<PaintLine>().Include(x => x.Brand).FirstOrDefault(x => x.Name == "Test Line");
        line.ShouldNotBeNull();

        var request = new CreatePaintColor.Request
        {
            Name = "Existing Color",
            HexColor = "#00FF00",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = line.Id,
            ManufacturerCode = "TC-02"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("/api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A paint color with this name already exists in this paint line.");

        // Assert the database - should still have only one color with this name
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Existing Color" && x.LineId == line.Id).ToList();
        colors.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 9)]
    public async Task User_Cannot_Create_Paint_Color_When_HexColor_Already_Exists_In_Same_Line()
    {
        // Prepare
        var line = TestingServer.CreateContext().Set<PaintLine>().Include(x => x.Brand).FirstOrDefault(x => x.Name == "Test Line");
        line.ShouldNotBeNull();

        var request = new CreatePaintColor.Request
        {
            Name = "Different Name",
            HexColor = "#FF0000", // This hex color already exists
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = line.Id,
            ManufacturerCode = "TC-02"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("/api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A paint color with this hex color already exists in this paint line.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Different Name").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 10)]
    public async Task User_Can_Create_Paint_Color()
    {
        // Prepare
        var line = TestingServer.CreateContext().Set<PaintLine>().Include(x => x.Brand).FirstOrDefault(x => x.Name == "Test Line");
        line.ShouldNotBeNull();

        var request = new CreatePaintColor.Request
        {
            Name = "New Color",
            HexColor = "#00FF00",
            BottleSize = 17.0,
            Type = PaintType.Metallic,
            LineId = line.Id,
            ManufacturerCode = "NC-01"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("/api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("New Color");
        result.HexColor.ShouldBe("#00FF00");
        result.BottleSize.ShouldBe(17.0);
        result.Type.ShouldBe(PaintType.Metallic);
        result.ManufacturerCode.ShouldBe("NC-01");

        // Assert the database
        var entity = TestingServer.CreateContext().Set<PaintColor>().Include(x => x.Line).ThenInclude(x => x.Brand).FirstOrDefault(x => x.Id == result.Id);

        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(result.Id);
        entity.Name.ShouldBe("New Color");
        entity.HexColor.ShouldBe("#00FF00");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
