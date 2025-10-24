namespace PaintingProjectsManagement.Features.Paints.Brands.Tests;

public class Delete_PaintBrand_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _brandId;
    private static Guid _brandWithLinesId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands for the tests
        var testBrand = new PaintBrand("Test Brand for Delete");
        var testBrandWithLines = new PaintBrand("Test Brand with Lines");
        
        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(testBrand);
            await context.AddAsync(testBrandWithLines);
            await context.SaveChangesAsync();
            _brandId = testBrand.Id;
            _brandWithLinesId = testBrandWithLines.Id;

            // Create a paint line for the second brand
            var paintLine = new PaintLine(testBrandWithLines, "Test Paint Line");
            await context.AddAsync(paintLine);
            await context.SaveChangesAsync();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Delete_PaintBrand()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/brands/{_brandId}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - brand should still exist
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Cannot_Delete_PaintBrand()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/brands/{_brandId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Forbidden);

        // Assert the database - brand should still exist
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Superuser_Cannot_Delete_PaintBrand_When_Id_Is_Invalid()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/brands/{Guid.NewGuid()}", "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - original brand should still exist
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Superuser_Cannot_Delete_PaintBrand_When_It_Has_Associated_Paint_Lines()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/brands/{_brandWithLinesId}", "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Cannot delete a paint brand that has associated paint lines. Remove the paint lines first.");

        // Assert the database - brand should still exist
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandWithLinesId);
        brand.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Superuser_Can_Delete_PaintBrand_When_No_Associated_Paint_Lines()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/brands/{_brandId}", "superuser");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - brand should be deleted
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldBeNull();
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 