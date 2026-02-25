namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

[HumanFriendlyDisplayName]
public class Delete_PaintBrand_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands
        var brandToDelete = new PaintBrand("Brand To Delete");
        var brandWithLines = new PaintBrand("Brand With Lines");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(brandToDelete);
            await context.AddAsync(brandWithLines);
            await context.SaveChangesAsync();

            // Create a paint line for the brandWithLines
            var paintLine = new PaintLine(brandWithLines, "Test Line");
            await context.AddAsync(paintLine);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedBrand = context.Set<PaintBrand>().FirstOrDefault(x => x.Name == "Brand To Delete");
            savedBrand.ShouldNotBeNull();

            var savedBrandWithLines = context.Set<PaintBrand>().FirstOrDefault(x => x.Name == "Brand With Lines");
            savedBrandWithLines.ShouldNotBeNull();

            var paintLine = context.Set<PaintLine>().FirstOrDefault(x => x.BrandId == savedBrandWithLines.Id);
            paintLine.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Delete_Paint_Brand()
    {
        // Prepare - Use a non-existent brand ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/brands/{nonExistentId}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Delete_Paint_Brand_That_Does_Not_Exist()
    {
        // Prepare - Use a non-existent brand ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/brands/{nonExistentId}", "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Delete_Paint_Brand_That_Has_Paint_Lines()
    {
        // Prepare - Load the brand that has associated paint lines
        var brandWithLines = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Brand With Lines");
        brandWithLines.ShouldNotBeNull("Brand should exist from seed");

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/brands/{brandWithLines.Id}", "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Cannot delete a paint brand that has associated paint lines. Remove the paint lines first.");

        // Assert the database - brand should still exist
        var stillExistingEntity = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == brandWithLines.Id);
        stillExistingEntity.ShouldNotBeNull("Brand should still exist in database");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Can_Delete_Paint_Brand()
    {
        // Prepare - Load the brand to delete
        var brandToDelete = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Brand To Delete");
        brandToDelete.ShouldNotBeNull("Brand should exist from seed");

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/brands/{brandToDelete.Id}", "superuser");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - brand should be deleted
        var deletedEntity = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == brandToDelete.Id);
        deletedEntity.ShouldBeNull("Brand should be deleted from database");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
