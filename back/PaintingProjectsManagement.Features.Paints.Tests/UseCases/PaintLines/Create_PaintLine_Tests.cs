namespace PaintingProjectsManagement.Features.Paints.Lines.Tests;

public class Create_PaintLine_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _testBrandId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");


        using (var context = TestingServer.CreateContext())
        {
            var testBrand = new PaintBrand("Test Brand");
            await context.AddAsync(testBrand);
            await context.SaveChangesAsync();
            _testBrandId = testBrand.Id;
        }
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_PaintLine()
    {
        // Prepare - Create a brand for this test
        var request = new CreatePaintLine.Request
        {
            BrandId = _testBrandId,
            Name = "Test Line"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("api/paints/lines", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == "Test Line").ToList();
        lines.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Cannot_Create_PaintLine()
    {
        // Prepare - Create a brand for this test
        var request = new CreatePaintLine.Request
        {
            BrandId = _testBrandId,
            Name = "Test Line"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("api/paints/lines", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Forbidden);

        // Assert the database
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == "Test Line").ToList();
        lines.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task Superuser_Cannot_Create_PaintLine_When_Name_Is_Empty(string? name)
    {
        // Prepare - Create a brand for this test
        var request = new CreatePaintLine.Request
        {
            BrandId = _testBrandId,
            Name = name!
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == name).ToList();
        lines.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Superuser_Cannot_Create_PaintLine_When_BrandId_Is_Invalid()
    {
        // Prepare
        var request = new CreatePaintLine.Request
        {
            BrandId = Guid.NewGuid(), // Invalid brand ID
            Name = "Test Line"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "BrandId references a non-existent record.");

        // Assert the database
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == "Test Line").ToList();
        lines.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Superuser_Cannot_Create_PaintLine_When_Name_Already_Exists_For_Same_Brand()
    {
        // Prepare 
        using (var context = TestingServer.CreateContext())
        {
            var brand = context.Set<PaintBrand>().FirstOrDefault(x => x.Id == _testBrandId);
            var existingLine = new PaintLine(brand, "Existing Line Test");

            await context.AddAsync(existingLine);
            await context.SaveChangesAsync();
        }   

        var request = new CreatePaintLine.Request
        {
            BrandId = _testBrandId,
            Name = "Existing Line Test"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A paint line with this name already exists for this brand.");

        // Assert the database - should still have only one line with this name for this brand
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.BrandId == _testBrandId && x.Name == "Existing Line Test").ToList();
        lines.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Superuser_Can_Create_PaintLine_When_Data_Is_Valid()
    {
        // Prepare - Create a brand for this test
        var request = new CreatePaintLine.Request
        {
            BrandId = _testBrandId,
            Name = "Test Line Valid"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Test Line Valid");
        result.Brand.ShouldNotBeNull();
        result.Brand.Id.ShouldBe(_testBrandId);

        // Assert the database
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Name == "Test Line Valid");
        line.ShouldNotBeNull();
        line.Id.ShouldBe(result.Id);
        line.Name.ShouldBe("Test Line Valid");
        line.BrandId.ShouldBe(_testBrandId);
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Superuser_Can_Create_PaintLine_With_Same_Name_For_Different_Brand()
    {
        var otherBrand = new PaintBrand("Another Brand");
        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(otherBrand);
            await context.SaveChangesAsync();
        }

        // Prepare - Create another brand
        var request1 = new CreatePaintLine.Request
        {
            BrandId = _testBrandId,
            Name = "Unique Name" 
        };

        var request2 = new CreatePaintLine.Request
        {
            BrandId = otherBrand.Id,
            Name = "Unique Name"
        };

        // Act
        var response1 = await TestingServer.PostAsync<PaintLineDetails>("api/paints/lines", request1, "superuser");
        var response2 = await TestingServer.PostAsync<PaintLineDetails>("api/paints/lines", request2, "superuser");

        // Assert the response
        response1.ShouldBeSuccess(out var result1);
        result1.ShouldNotBeNull();
        result1.Id.ShouldNotBe(Guid.Empty);
        result1.Name.ShouldBe("Unique Name");
        result1.Brand.ShouldNotBeNull();
        result1.Brand.Id.ShouldBe(_testBrandId);

        response2.ShouldBeSuccess(out var result2);
        result2.ShouldNotBeNull();
        result2.Id.ShouldNotBe(Guid.Empty);
        result2.Name.ShouldBe("Unique Name");
        result2.Brand.ShouldNotBeNull();
        result2.Brand.Id.ShouldBe(otherBrand.Id);

        // Assert the database
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == "Unique Name").ToList();
        lines.ShouldNotBeNull();
        lines.Count.ShouldBe(2);    
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}