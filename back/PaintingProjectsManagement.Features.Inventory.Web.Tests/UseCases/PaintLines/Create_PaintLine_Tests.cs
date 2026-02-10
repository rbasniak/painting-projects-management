namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

public class Create_PaintLine_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brand and existing line
        var brand = new PaintBrand("Test Brand");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(brand);
            await context.SaveChangesAsync();

            var existingLine = new PaintLine(brand, "Existing Line");
            await context.AddAsync(existingLine);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedBrand = context.Set<PaintBrand>().FirstOrDefault(x => x.Name == "Test Brand");
            savedBrand.ShouldNotBeNull();

            var savedLine = context.Set<PaintLine>().FirstOrDefault(x => x.Name == "Existing Line");
            savedLine.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_Paint_Line()
    {
        // Prepare
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Test Brand");
        brand.ShouldNotBeNull();

        var request = new CreatePaintLine.Request
        {
            BrandId = brand.Id,
            Name = "Test Line"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("/api/paints/lines", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == "Test Line").ToList();
        lines.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 3)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Create_Paint_Line_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Test Brand");
        brand.ShouldNotBeNull();

        var request = new CreatePaintLine.Request
        {
            BrandId = brand.Id,
            Name = name!
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("/api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == name).ToList();
        lines.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Create_Paint_Line_When_Name_Exceeds_MaxLength()
    {
        // Prepare
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Test Brand");
        brand.ShouldNotBeNull();

        var request = new CreatePaintLine.Request
        {
            BrandId = brand.Id,
            Name = new string('A', 101) // Exceeds max length of 100
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("/api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == new string('A', 101)).ToList();
        lines.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Create_Paint_Line_When_BrandId_Does_Not_Exist()
    {
        // Prepare
        var nonExistentBrandId = Guid.NewGuid();

        var request = new CreatePaintLine.Request
        {
            BrandId = nonExistentBrandId,
            Name = "Test Line"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("/api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "BrandId references a non-existent record.");

        // Assert the database
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == "Test Line").ToList();
        lines.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Create_Paint_Line_When_Name_Already_Exists_For_Same_Brand()
    {
        // Prepare
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Test Brand");
        brand.ShouldNotBeNull();

        var request = new CreatePaintLine.Request
        {
            BrandId = brand.Id,
            Name = "Existing Line"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("/api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A paint line with this name already exists for this brand.");

        // Assert the database - should still have only one line with this name for this brand
        var lines = TestingServer.CreateContext().Set<PaintLine>().Where(x => x.Name == "Existing Line" && x.BrandId == brand.Id).ToList();
        lines.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Can_Create_Paint_Line()
    {
        // Prepare
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Test Brand");
        brand.ShouldNotBeNull();

        var request = new CreatePaintLine.Request
        {
            BrandId = brand.Id,
            Name = "New Line"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintLineDetails>("/api/paints/lines", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("New Line");
        result.Brand.Id.ShouldBe(brand.Id);

        // Assert the database
        var entity = TestingServer.CreateContext().Set<PaintLine>().Include(x => x.Brand).FirstOrDefault(x => x.Id == result.Id);

        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(result.Id);
        entity.Name.ShouldBe("New Line");
        entity.BrandId.ShouldBe(brand.Id);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
