namespace PaintingProjectsManagement.Features.Paints.Tests;

public class Update_PaintColor_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _lineId;
    private static Guid _line2Id;
    private static Guid _colorId;
    private static Guid _color2Id;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test paint brands and lines for the tests
        var brand1 = new PaintBrand("Test Brand 1");
        var brand2 = new PaintBrand("Test Brand 2");

        var line1 = new PaintLine(brand1, "Test Line 1");
        var line2 = new PaintLine(brand2, "Test Line 2");

        var color1 = new PaintColor(
            line: line1,
            name: "Test Color 1",
            hexColor: "#FF0000",
            bottleSize: 17.0,
            type: PaintType.Opaque,
            manufacturerCode: "TEST1"
        );

        var color2 = new PaintColor(
            line: line2,
            name: "Test Color 2",
            hexColor: "#00FF00",
            bottleSize: 12.0,
            type: PaintType.Metallic,
            manufacturerCode: "TEST2"
        );

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(brand1);
            await context.AddAsync(line1);
            await context.AddAsync(color1);
            await context.SaveChangesAsync();
            _lineId = line1.Id;
            _colorId = color1.Id;

            await context.AddAsync(brand2);
            await context.AddAsync(line2);
            await context.AddAsync(color2);
            await context.SaveChangesAsync();
            _line2Id = line2.Id;
            _color2Id = color2.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Update_PaintColor()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Updated Test Color",
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Cannot_Update_PaintColor()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Updated Test Color",
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Forbidden);

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Superuser_Cannot_Update_PaintColor_When_Id_Is_Invalid()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = Guid.NewGuid(), // Invalid ID
            Name = "Updated Test Color",
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - no colors should be created
        var colors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Updated Test Color").ToList();
        colors.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Superuser_Cannot_Update_PaintColor_When_Line_Is_Invalid()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Updated Test Color",
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = Guid.NewGuid(), // Invalid line ID
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "LineId references a non-existent record.");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 6)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task Superuser_Cannot_Update_PaintColor_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = name!,
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Superuser_Cannot_Update_PaintColor_When_Name_Max_Length_Is_Exceeded()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = new string('A', 101), // Exceeds max length
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 8)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task Superuser_Cannot_Update_PaintColor_When_HexColor_Is_Empty(string? hexColor)
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Updated Test Color",
            HexColor = hexColor!,
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "HexColor is required.");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 9)]
    [Arguments("FF0000")] // Missing #
    [Arguments("#FF000")] // Too short
    [Arguments("#GG0000")] // Invalid characters
    public async Task Superuser_Cannot_Update_PaintColor_When_HexColor_Format_Is_Invalid(string hexColor)
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Updated Test Color",
            HexColor = hexColor,
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Hex color must be in format #RRGGBB");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 9)]
    public async Task Superuser_Cannot_Update_PaintColor_When_HexColor_Is_Too_Long()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Updated Test Color",
            HexColor = "#FF00000", // Too long - 8 characters
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response - database constraint takes precedence
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "HexColor cannot exceed 7 characters.");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 10)]
    public async Task Superuser_Cannot_Update_PaintColor_When_BottleSize_Is_Zero()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Updated Test Color",
            HexColor = "#FF0000",
            BottleSize = 0.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Bottle size must be greater than zero.");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 11)]
    public async Task Superuser_Cannot_Update_PaintColor_When_BottleSize_Is_Negative()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Updated Test Color",
            HexColor = "#FF0000",
            BottleSize = -17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Bottle size must be greater than zero.");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 12)]
    public async Task Superuser_Cannot_Update_PaintColor_When_Type_Is_Invalid()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Updated Test Color",
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = (PaintType)999, // Invalid enum value
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Type has an invalid value.");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 13)]
    public async Task Superuser_Cannot_Update_PaintColor_When_Name_Already_Exists_In_Same_Line()
    {
        // Prepare - Create another color in the same line
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

        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Existing Color", // Try to use the same name as existing color
            HexColor = "#FF0000",
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Another paint color with this name already exists in this paint line.");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Test Color 1"); // Original name should remain

        // Verify no duplicate was created
        var finalColors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.Name == "Existing Color").ToList();
        finalColors.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 14)]
    public async Task Superuser_Cannot_Update_PaintColor_When_HexColor_Already_Exists_In_Same_Line()
    {
        // Prepare - Create another color in the same line
        using (var context = TestingServer.CreateContext())
        {
            var existingColor = new PaintColor(
                line: context.Set<PaintLine>().First(x => x.Id == _lineId),
                name: "Existing Color",
                hexColor: "#00FFFF",
                bottleSize: 17.0,
                type: PaintType.Metallic,
                manufacturerCode: "EXIST001"
            );

            await context.AddAsync(existingColor);
            await context.SaveChangesAsync();
        }

        // Verify the existing color is in the database
        var initialColors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.HexColor == "#00FFFF").ToList();
        initialColors.Count.ShouldBe(1);

        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Different Color Name",
            HexColor = "#00FFFF", // Try to use the same hex color as existing color
            BottleSize = 17.0,
            Type = PaintType.Opaque,
            LineId = _lineId,
            ManufacturerCode = "TEST001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Another paint color with this hex color already exists in this paint line.");

        // Assert the database - color should not be updated
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.HexColor.ShouldBe("#FF0000"); // Original hex color should remain

        // Verify no duplicate was created
        var finalColors = TestingServer.CreateContext().Set<PaintColor>().Where(x => x.HexColor == "#00FFFF").ToList();
        finalColors.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 15)]
    public async Task Superuser_Can_Update_PaintColor_When_Data_Is_Valid()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Updated Valid Color",
            HexColor = "#0000FF",
            BottleSize = 24.0,
            Type = PaintType.Wash,
            LineId = _lineId,
            ManufacturerCode = "UPDATED001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_colorId);
        result.Name.ShouldBe("Updated Valid Color");
        result.HexColor.ShouldBe("#0000FF");
        result.BottleSize.ShouldBe(24.0);
        result.Type.ShouldBe(PaintType.Wash);
        result.Line.Id.ShouldBe(_lineId);
        result.ManufacturerCode.ShouldBe("UPDATED001");

        // Assert the database
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Updated Valid Color");
        color.HexColor.ShouldBe("#0000FF");
        color.BottleSize.ShouldBe(24.0);
        color.Type.ShouldBe(PaintType.Wash);
        color.LineId.ShouldBe(_lineId);
        color.ManufacturerCode.ShouldBe("UPDATED001");
    }

    [Test, NotInParallel(Order = 16)]
    public async Task Superuser_Can_Update_PaintColor_Without_ManufacturerCode()
    {
        // Prepare
        var request = new UpdatePaintColor.Request
        {
            Id = _color2Id,
            Name = "Updated Color Without Code",
            HexColor = "#FFFF00",
            BottleSize = 15.0,
            Type = PaintType.Ink,
            LineId = _line2Id,
            ManufacturerCode = null
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Updated Color Without Code");
        result.ManufacturerCode.ShouldBeNull();

        // Assert the database
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _color2Id);
        color.ShouldNotBeNull();
        color.ManufacturerCode.ShouldBeNull();
    }

    [Test, NotInParallel(Order = 17)]
    public async Task Superuser_Can_Update_PaintColor_To_Different_Line()
    {
        // Prepare - Create a third line
        var thirdBrand = new PaintBrand("Third Brand");
        var thirdLine = new PaintLine(thirdBrand, "Third Line");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(thirdBrand);
            await context.AddAsync(thirdLine);
            await context.SaveChangesAsync();
        }

        var request = new UpdatePaintColor.Request
        {
            Id = _colorId,
            Name = "Color Moved To New Line",
            HexColor = "#FF00FF",
            BottleSize = 20.0,
            Type = PaintType.Contrast,
            LineId = thirdLine.Id,
            ManufacturerCode = "MOVED001"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintColorDetails>("api/paints/colors", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Line.Id.ShouldBe(thirdLine.Id);

        // Assert the database
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _colorId);
        color.ShouldNotBeNull();
        color.LineId.ShouldBe(thirdLine.Id);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 
