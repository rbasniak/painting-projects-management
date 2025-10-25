namespace PaintingProjectsManagement.Features.Paints.Tests;

public class Delete_PaintColor_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _color1Id;
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
            _color1Id = color1.Id;

            await context.AddAsync(brand2);
            await context.AddAsync(line2);
            await context.AddAsync(color2);
            await context.SaveChangesAsync();
            _color2Id = color2.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Delete_PaintColor()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/colors/{_color1Id}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - color should still exist
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _color1Id);
        color.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Cannot_Delete_PaintColor()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/colors/{_color1Id}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Forbidden);

        // Assert the database - color should still exist
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _color1Id);
        color.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Superuser_Cannot_Delete_PaintColor_When_Id_Is_Invalid()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/colors/{Guid.NewGuid()}", "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - all colors should still exist
        var colors = TestingServer.CreateContext().Set<PaintColor>().ToList();
        colors.Count.ShouldBe(2);
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Superuser_Can_Delete_PaintColor_When_Id_Is_Valid()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/paints/colors/{_color1Id}", "superuser");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - color should be deleted
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Id == _color1Id);
        color.ShouldBeNull();

        // Assert that other colors still exist
        var remainingColors = TestingServer.CreateContext().Set<PaintColor>().ToList();
        remainingColors.Count.ShouldBe(1);
        remainingColors.First().Id.ShouldBe(_color2Id);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 
