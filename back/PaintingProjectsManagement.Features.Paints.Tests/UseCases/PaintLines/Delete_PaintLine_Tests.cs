namespace PaintingProjectsManagement.Features.Paints.Lines.Tests;

[HumanFriendlyDisplayName]
public class Delete_PaintLine_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _lineId;
    private static Guid _lineWithColorsId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands and lines for the tests
        var testBrand = new PaintBrand("Test Brand for Delete");
        var testBrandWithColors = new PaintBrand("Test Brand with Colors");
        
        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(testBrand);
            await context.AddAsync(testBrandWithColors);
            await context.SaveChangesAsync();

            // Create test lines
            var testLine = new PaintLine(testBrand, "Test Line for Delete");
            var testLineWithColors = new PaintLine(testBrandWithColors, "Test Line with Colors");
            await context.AddAsync(testLine);
            await context.AddAsync(testLineWithColors);
            await context.SaveChangesAsync();
            _lineId = testLine.Id;
            _lineWithColorsId = testLineWithColors.Id;

            // Create a paint color for the second line
            var paintColor = new PaintColor(testLineWithColors, "Test Color", "FF0000", 17.0, PaintType.Opaque);
            await context.AddAsync(paintColor);
            await context.SaveChangesAsync();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Delete_PaintLine()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/lines/{_lineId}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - line should still exist
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Cannot_Delete_PaintLine()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/lines/{_lineId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Forbidden);

        // Assert the database - line should still exist
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Superuser_Cannot_Delete_PaintLine_When_Id_Is_Invalid()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/lines/{Guid.NewGuid()}", "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - original line should still exist
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Superuser_Cannot_Delete_PaintLine_When_It_Has_Associated_Paint_Colors()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/lines/{_lineWithColorsId}", "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Cannot delete a paint line that has associated paint colors. Remove the paint colors first.");

        // Assert the database - line should still exist
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineWithColorsId);
        line.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Superuser_Can_Delete_PaintLine_When_No_Associated_Paint_Colors()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/lines/{_lineId}", "superuser");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - line should be deleted
        var line = TestingServer.CreateContext().Set<PaintLine>().FirstOrDefault(x => x.Id == _lineId);
        line.ShouldBeNull();
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 