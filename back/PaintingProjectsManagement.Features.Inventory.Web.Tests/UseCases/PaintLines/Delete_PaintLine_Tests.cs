namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

public class Delete_PaintLine_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brand and lines
        var brand = new PaintBrand("Test Brand");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(brand);
            await context.SaveChangesAsync();

            var lineToDelete = new PaintLine(brand, "Line To Delete");
            var lineWithColors = new PaintLine(brand, "Line With Colors");

            await context.AddAsync(lineToDelete);
            await context.AddAsync(lineWithColors);
            await context.SaveChangesAsync();

            // Create a paint color for the lineWithColors
            var paintColor = new PaintColor(lineWithColors, "Test Color", "#FF0000", 17.0, PaintType.Opaque, "TC-01");
            await context.AddAsync(paintColor);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedLine = context.Set<PaintLine>().FirstOrDefault(x => x.Name == "Line To Delete");
            savedLine.ShouldNotBeNull();

            var savedLineWithColors = context.Set<PaintLine>().FirstOrDefault(x => x.Name == "Line With Colors");
            savedLineWithColors.ShouldNotBeNull();

            var paintColor = context.Set<PaintColor>().FirstOrDefault(x => x.LineId == savedLineWithColors.Id);
            paintColor.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Delete_Paint_Line()
    {
        // Prepare - Use a non-existent line ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/lines/{nonExistentId}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Delete_Paint_Line_That_Does_Not_Exist()
    {
        // Prepare - Use a non-existent line ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/lines/{nonExistentId}", "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Delete_Paint_Line_That_Has_Paint_Colors()
    {
        // Prepare - Load the line that has associated paint colors
        var lineWithColors = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Name == "Line With Colors");
        lineWithColors.ShouldNotBeNull("Line should exist from seed");

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/lines/{lineWithColors.Id}", "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Cannot delete a paint line that has associated paint colors. Remove the paint colors first.");

        // Assert the database - line should still exist
        var stillExistingEntity = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == lineWithColors.Id);
        stillExistingEntity.ShouldNotBeNull("Line should still exist in database");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Can_Delete_Paint_Line()
    {
        // Prepare - Load the line to delete
        var lineToDelete = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Name == "Line To Delete");
        lineToDelete.ShouldNotBeNull("Line should exist from seed");

        // Act
        var response = await TestingServer.DeleteAsync($"/api/paints/lines/{lineToDelete.Id}", "superuser");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - line should be deleted
        var deletedEntity = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == lineToDelete.Id);
        deletedEntity.ShouldBeNull("Line should be deleted from database");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
