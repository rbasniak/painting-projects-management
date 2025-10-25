namespace PaintingProjectsManagement.Features.Paints.Lines.Tests;

public class Update_PaintLine_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _lineId;
    private static Guid _brandId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands and line for the tests
        var testBrand = new PaintBrand("Test Brand for Update");
        var testLine = new PaintLine(testBrand, "Test Line for Update");
        
        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(testBrand);
            await context.AddAsync(testLine);
            await context.SaveChangesAsync();
            _lineId = testLine.Id;
            _brandId = testBrand.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Update_PaintLine()
    {
        // Prepare
        var request = new UpdatePaintLine.Request
        {
            Id = _lineId,
            Name = "Updated Line Name"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintLineDetails>("api/paints/lines", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - name should not be changed
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldNotBeNull();
        line.Name.ShouldBe("Test Line for Update");
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Cannot_Update_PaintLine()
    {
        // Prepare
        var request = new UpdatePaintLine.Request
        {
            Id = _lineId,
            Name = "Updated Line Name"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintLineDetails>("api/paints/lines", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Forbidden);

        // Assert the database - name should not be changed
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldNotBeNull();
        line.Name.ShouldBe("Test Line for Update");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Superuser_Cannot_Update_PaintLine_When_Id_Is_Invalid()
    {
        // Prepare
        var request = new UpdatePaintLine.Request
        {
            Id = Guid.NewGuid(), // Invalid ID
            Name = "Updated Line Name"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintLineDetails>("api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - original line should not be changed
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldNotBeNull();
        line.Name.ShouldBe("Test Line for Update");
    }

    [Test, NotInParallel(Order = 6)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task Superuser_Cannot_Update_PaintLine_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var request = new UpdatePaintLine.Request
        {
            Id = _lineId,
            Name = name!
        };

        // Act
        var response = await TestingServer.PutAsync<PaintLineDetails>("api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database - name should not be changed
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldNotBeNull();
        line.Name.ShouldBe("Test Line for Update");
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Superuser_Cannot_Update_PaintLine_When_Name_Already_Exists_For_Same_Brand()
    {
        // Prepare - Create another line with the same brand first
        using (var context = TestingServer.CreateContext())
        {
            var brand = await context.Set<PaintBrand>().FirstOrDefaultAsync(x => x.Id == _brandId);
            var existingLine = new PaintLine(brand, "Existing Line Name");
            await context.AddAsync(existingLine);
            await context.SaveChangesAsync();
        }

        var request = new UpdatePaintLine.Request
        {
            Id = _lineId,
            Name = "Existing Line Name"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintLineDetails>("api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Another paint line with this name already exists for this brand.");

        // Assert the database - name should not be changed
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldNotBeNull();
        line.Name.ShouldBe("Test Line for Update");
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Superuser_Can_Update_PaintLine_When_Data_Is_Valid()
    {
        // Prepare
        var request = new UpdatePaintLine.Request
        {
            Id = _lineId,
            Name = "Updated Line Name Valid"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintLineDetails>("api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_lineId);
        result.Name.ShouldBe("Updated Line Name Valid");
        result.Brand.ShouldNotBeNull();
        result.Brand.Id.ShouldBe(_brandId);

        // Assert the database
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldNotBeNull();
        line.Name.ShouldBe("Updated Line Name Valid");
        line.BrandId.ShouldBe(_brandId);
    }

    [Test, NotInParallel(Order = 9)]
    public async Task Superuser_Can_Update_PaintLine_To_Same_Name()
    {
        // Prepare
        var request = new UpdatePaintLine.Request
        {
            Id = _lineId,
            Name = "Updated Line Name Valid" // Same name as previous test
        };

        // Act
        var response = await TestingServer.PutAsync<PaintLineDetails>("api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_lineId);
        result.Name.ShouldBe("Updated Line Name Valid");
        result.Brand.ShouldNotBeNull();
        result.Brand.Id.ShouldBe(_brandId);

        // Assert the database
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldNotBeNull();
        line.Name.ShouldBe("Updated Line Name Valid");
        line.BrandId.ShouldBe(_brandId);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 