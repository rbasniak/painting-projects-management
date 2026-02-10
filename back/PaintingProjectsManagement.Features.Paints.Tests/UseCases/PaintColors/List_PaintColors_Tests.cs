namespace PaintingProjectsManagement.Features.Paints.Tests;

[HumanFriendlyDisplayName]
public class List_PaintColors_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _brand1Id;
    private static Guid _line1Id;
    private static Guid _color1Id;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test paint brands and lines for the tests
        var brand1 = new PaintBrand("Brand A");
        var brand2 = new PaintBrand("Brand B");

        var line1 = new PaintLine(brand1, "Line A1");
        var line2 = new PaintLine(brand1, "Line A2");
        var line3 = new PaintLine(brand2, "Line B1");

        var color1 = new PaintColor(
            line: line1,
            name: "Color A1-1",
            hexColor: "#FF0000",
            bottleSize: 17.0,
            type: PaintType.Opaque,
            manufacturerCode: "A1-1"
        );

        var color2 = new PaintColor(
            line: line1,
            name: "Color A1-2",
            hexColor: "#00FF00",
            bottleSize: 12.0,
            type: PaintType.Metallic,
            manufacturerCode: "A1-2"
        );

        var color3 = new PaintColor(
            line: line2,
            name: "Color A2-1",
            hexColor: "#0000FF",
            bottleSize: 24.0,
            type: PaintType.Wash,
            manufacturerCode: "A2-1"
        );

        var color4 = new PaintColor(
            line: line3,
            name: "Color B1-1",
            hexColor: "#FFFF00",
            bottleSize: 15.0,
            type: PaintType.Ink,
            manufacturerCode: "B1-1"
        );

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(brand1);
            await context.AddAsync(brand2);
            await context.SaveChangesAsync();
            _brand1Id = brand1.Id;

            await context.AddAsync(line1);
            await context.AddAsync(line2);
            await context.AddAsync(line3);
            await context.SaveChangesAsync();
            _line1Id = line1.Id;

            await context.AddAsync(color1);
            await context.AddAsync(color2);
            await context.AddAsync(color3);
            await context.AddAsync(color4);
            await context.SaveChangesAsync();
            _color1Id = color1.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_PaintColors()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintColorDetails>>("api/paints/colors");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Authenticated_User_Can_List_All_PaintColors()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintColorDetails>>("api/paints/colors", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        var result = response.Data;
        result.ShouldNotBeNull();
        result.Count.ShouldBe(4);

        // Assert the colors are ordered correctly (by brand name, then line name, then color name)
        var colors = result.ToList();
        colors[0].Name.ShouldBe("Color A1-1"); // Brand A, Line A1, Color A1-1
        colors[1].Name.ShouldBe("Color A1-2"); // Brand A, Line A1, Color A1-2
        colors[2].Name.ShouldBe("Color A2-1"); // Brand A, Line A2, Color A2-1
        colors[3].Name.ShouldBe("Color B1-1"); // Brand B, Line B1, Color B1-1
    }

    [Test, NotInParallel(Order = 4)]
    public async Task PaintColor_Details_Are_Correctly_Mapped()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintColorDetails>>("api/paints/colors", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        var result = response.Data;
        result.ShouldNotBeNull();

        // Find the first color and verify its details
        var color = result.FirstOrDefault(x => x.Id == _color1Id);
        color.ShouldNotBeNull();
        color.Name.ShouldBe("Color A1-1");
        color.HexColor.ShouldBe("#FF0000");
        color.BottleSize.ShouldBe(17.0);
        color.Type.ShouldBe(PaintType.Opaque);
        color.ManufacturerCode.ShouldBe("A1-1");
        color.Line.ShouldNotBeNull();
        color.Line.Id.ShouldBe(_line1Id);
        color.Line.Name.ShouldBe("Line A1");
        color.Brand.ShouldNotBeNull();
        color.Brand.Id.ShouldBe(_brand1Id);
        color.Brand.Name.ShouldBe("Brand A");
    } 

    [Test, NotInParallel(Order = 10)]
    public async Task Different_Users_See_Same_Results()
    {
        // Act
        var response1 = await TestingServer.GetAsync<IReadOnlyCollection<PaintColorDetails>>("api/paints/colors", "rodrigo.basniak");
        var response2 = await TestingServer.GetAsync<IReadOnlyCollection<PaintColorDetails>>("api/paints/colors", "ricardo.smarzaro");

        // Assert the responses
        response1.ShouldBeSuccess();
        response2.ShouldBeSuccess();
        var result1 = response1.Data;
        var result2 = response2.Data;

        result1.Count.ShouldBe(result2.Count);
        result1.Count.ShouldBe(4);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 