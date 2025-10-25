namespace PaintingProjectsManagement.Features.Paints.Tests;

public class Create_PaintColor_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _lineId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test paint brand and line for the tests
        var brand = new PaintBrand("Test Brand");
        var line = new PaintLine(brand, "Test Line");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(brand);
            await context.AddAsync(line);
            await context.SaveChangesAsync();
            _lineId = line.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_PaintColor()
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Cannot_Create_PaintColor()
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Forbidden);

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Superuser_Cannot_Create_PaintColor_When_Line_Is_Invalid()
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = Guid.NewGuid(), // Invalid line ID
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "LineId references a non-existent record.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task Superuser_Cannot_Create_PaintColor_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = name!,
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == name).ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Superuser_Cannot_Create_PaintColor_When_Name_Max_Length_Is_Exceeded()
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = new string('A', 101), // Exceeds max length
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == new string('A', 101)).ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 7)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task Superuser_Cannot_Create_PaintColor_When_HexColor_Is_Empty(string? hexColor)
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = hexColor!,
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "HexColor is required.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 8)]
    [Arguments("FF0000")] // Missing #
    [Arguments("#FF000")] // Too short
    [Arguments("#GG0000")] // Invalid characters
    public async Task Superuser_Cannot_Create_PaintColor_When_HexColor_Format_Is_Invalid(string hexColor)
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = hexColor,
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Hex color must be in format #RRGGBB");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Superuser_Cannot_Create_PaintColor_When_HexColor_Is_Too_Long()
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "#FF00000", // Too long - 8 characters
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response - database constraint takes precedence
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "HexColor cannot exceed 7 characters.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Superuser_Can_Create_PaintColor_When_HexColor_Is_Lowercase()
    {
        // Prepare - lowercase hex color should be valid
        var request = new CreatePaintColor.Request
        {
            Name = "Test Color Lowercase",
            HexColor = "#ff0000", // Lowercase is valid
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.HexColor.ShouldBe("#ff0000");

        // Assert the database
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Name == "Test Color Lowercase");
        color.ShouldNotBeNull();
        color.HexColor.ShouldBe("#ff0000");
    }

    [Test, NotInParallel(Order = 9)]
    public async Task Superuser_Cannot_Create_PaintColor_When_BottleSize_Is_Zero()
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "#FF0000",
            BottleSize = 0.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Bottle size must be greater than zero.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 10)]
    public async Task Superuser_Cannot_Create_PaintColor_When_BottleSize_Is_Negative()
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "#FF0000",
            BottleSize = -17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Bottle size must be greater than zero.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 11)]
    public async Task Superuser_Cannot_Create_PaintColor_When_Type_Is_Invalid()
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Test Color",
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = (PaintType)999, // Invalid enum value
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Type has an invalid value.");

        // Assert the database
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 12)]
    public async Task Superuser_Cannot_Create_PaintColor_When_Name_Already_Exists_In_Same_Line()
    {
        // Prepare - Create a color first
        using (var context = TestingServer.CreateContext())
        {
            var existingColor = new PaintColor(
            line: context.Set<PaintLine>().First(x => x.Id == _lineId),
            name: "Existing Color",
            hexColor: "#00FF00",
            bottleSize: 17.0,
            type: PaintType.Metallic,
            manufacturerCode: "EXIST001"
        );

            await context.AddAsync(existingColor);
            await context.SaveChangesAsync();
        }

        // Verify the existing color is in the database
        var initialColors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Existing Color").ToList();
        initialColors.Count.ShouldBe(1);

        var request = new CreatePaintColor.Request
        {
            Name = "Existing Color",
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A paint color with this name already exists in this paint line.");

        // Assert the database - should still have only one color with this name
        var finalColors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Existing Color").ToList();
        finalColors.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 13)]
    public async Task Superuser_Cannot_Create_PaintColor_When_HexColor_Already_Exists_In_Same_Line()
    {
        // Prepare - Create a color first
        using (var context = TestingServer.CreateContext())
        {
            var existingColor = new PaintColor(
                line: context.Set<PaintLine>().First(x => x.Id == _lineId),
                name: "Existing Color 13",
                hexColor: "#FF0080",
                bottleSize: 17.0,
                type: PaintType.Metallic,
                manufacturerCode: "EXIST001"
            );

            await context.AddAsync(existingColor);
            await context.SaveChangesAsync();
        }

        // Verify the existing color is in the database
        var initialColors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.HexColor == "#FF0080").ToList();
        initialColors.Count.ShouldBe(1);

        var request = new CreatePaintColor.Request
        {
            Name = "Different Color Name",
            HexColor = "#FF0080", // Same hex color as existing
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A paint color with this hex color already exists in this paint line.");

        // Assert the database - should still have only one color with this hex color
        var finalColors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.HexColor == "#FF0080").ToList();
        finalColors.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 15)]
    public async Task Superuser_Can_Create_PaintColor_When_Data_Is_Valid()
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Valid Test Color",
            HexColor = "#FF0001",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "VALID001"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Valid Test Color");
        result.HexColor.ShouldBe("#FF0001");
        result.BottleSize.ShouldBe(17.0);
        result.Type.ShouldBe(PaintType.Opaque);
        result.Line.Id.ShouldBe(_lineId);
        result.ManufacturerCode.ShouldBe("VALID001");

        // Assert the database
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Name == "Valid Test Color");
        color.ShouldNotBeNull();
        color.Id.ShouldBe(result.Id);
        color.Name.ShouldBe("Valid Test Color");
        color.HexColor.ShouldBe("#FF0001");
        color.BottleSize.ShouldBe(17.0);
        color.Type.ShouldBe(PaintType.Opaque);
        color.LineId.ShouldBe(_lineId);
        color.ManufacturerCode.ShouldBe("VALID001");
    }

    [Test, NotInParallel(Order = 16)]
    public async Task Superuser_Can_Create_PaintColor_Without_ManufacturerCode()
    {
        // Prepare
        var request = new CreatePaintColor.Request
        {
            Name = "Color Without Code",
            HexColor = "#0000FF",
            BottleSize = 12.0,
            Type = PaintType.Metallic,
            LineId = _lineId,
            ManufacturerCode = null
        };

        // Act
        var response = await TestingServer.PostAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Color Without Code");
        result.ManufacturerCode.ShouldBeNull();

        // Assert the database
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Name == "Color Without Code");
        color.ShouldNotBeNull();
        color.ManufacturerCode.ShouldBeNull();
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 
