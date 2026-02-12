namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

[HumanFriendlyDisplayName]
public class Delete_PaintColor_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brand, line, and color
        var brand = new PaintBrand("Test Brand");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(brand);
            await context.SaveChangesAsync();

            var line = new PaintLine(brand, "Test Line");
            await context.AddAsync(line);
            await context.SaveChangesAsync();

            var colorToDelete = new PaintColor(line, "Color To Delete", "#FF0000", 17.0, PaintType.Opaque, "CTD-01");
            await context.AddAsync(colorToDelete);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedColor = context.Set<PaintColor>().FirstOrDefault(x => x.Name == "Color To Delete");
            savedColor.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Delete_Paint_Color()
    {
        // Prepare - Use a non-existent color ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/colors/{nonExistentId}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Delete_Paint_Color_That_Does_Not_Exist()
    {
        // Prepare - Use a non-existent color ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/colors/{nonExistentId}", "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Can_Delete_Paint_Color()
    {
        // Prepare - Load the color to delete
        var colorToDelete = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Name == "Color To Delete");
        colorToDelete.ShouldNotBeNull("Color should exist from seed");

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/colors/{colorToDelete.Id}", "superuser");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - color should be deleted
        var deletedEntity = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == colorToDelete.Id);
        deletedEntity.ShouldBeNull("Color should be deleted from database");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
