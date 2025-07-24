namespace PaintingProjectsManagement.Features.Paints.Lines.Tests;

public class List_PaintLines_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _brand1Id;
    private static Guid _brand2Id;
    private static Guid _line1Id;
    private static Guid _line2Id;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands and lines for the tests
        var testBrand1 = new PaintBrand("Brand A");
        var testBrand2 = new PaintBrand("Brand B");
        
        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(testBrand1);
            await context.AddAsync(testBrand2);
            await context.SaveChangesAsync();
            _brand1Id = testBrand1.Id;
            _brand2Id = testBrand2.Id;

            // Create test lines
            var testLine1 = new PaintLine(testBrand1, "Line A");
            var testLine2 = new PaintLine(testBrand2, "Line B");
            await context.AddAsync(testLine1);
            await context.AddAsync(testLine2);
            await context.SaveChangesAsync();
            _line1Id = testLine1.Id;
            _line2Id = testLine2.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_PaintLines()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintLineDetails>>("api/paints/lines");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Can_List_PaintLines()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintLineDetails>>("api/paints/lines", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        var lines = response.Data;
        lines.ShouldNotBeNull();
        lines.Count.ShouldBeGreaterThanOrEqualTo(2);

        // Verify the lines are returned in correct order (by brand name, then by line name)
        lines.ShouldBeEquivalentTo(lines.OrderBy(x => x.Brand.Name).ThenBy(x => x.Name).ToList());

        // Verify our test lines are included
        var lineA = lines.FirstOrDefault(x => x.Name == "Line A");
        lineA.ShouldNotBeNull();
        lineA.Id.ShouldBe(_line1Id);
        lineA.Brand.ShouldNotBeNull();
        lineA.Brand.Id.ShouldBe(_brand1Id);
        lineA.Brand.Name.ShouldBe("Brand A");

        var lineB = lines.FirstOrDefault(x => x.Name == "Line B");
        lineB.ShouldNotBeNull();
        lineB.Id.ShouldBe(_line2Id);
        lineB.Brand.ShouldNotBeNull();
        lineB.Brand.Id.ShouldBe(_brand2Id);
        lineB.Brand.Name.ShouldBe("Brand B");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Superuser_Can_List_PaintLines()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<PaintLineDetails>>("api/paints/lines", "superuser");

        // Assert the response
        response.ShouldBeSuccess();
        var lines = response.Data;
        lines.ShouldNotBeNull();
        lines.Count.ShouldBeGreaterThanOrEqualTo(2);

        // Verify the lines are returned in correct order (by brand name, then by line name)
        lines.ShouldBeEquivalentTo(lines.OrderBy(x => x.Brand.Name).ThenBy(x => x.Name).ToList());

        // Verify our test lines are included
        var lineA = lines.FirstOrDefault(x => x.Name == "Line A");
        lineA.ShouldNotBeNull();
        lineA.Id.ShouldBe(_line1Id);
        lineA.Brand.ShouldNotBeNull();
        lineA.Brand.Id.ShouldBe(_brand1Id);
        lineA.Brand.Name.ShouldBe("Brand A");

        var lineB = lines.FirstOrDefault(x => x.Name == "Line B");
        lineB.ShouldNotBeNull();
        lineB.Id.ShouldBe(_line2Id);
        lineB.Brand.ShouldNotBeNull();
        lineB.Brand.Id.ShouldBe(_brand2Id);
        lineB.Brand.Name.ShouldBe("Brand B");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Different_Users_Get_Same_List_Of_PaintLines()
    {
        // Act
        var response1 = await TestingServer.GetAsync<IReadOnlyCollection<PaintLineDetails>>("api/paints/lines", "rodrigo.basniak");
        var response2 = await TestingServer.GetAsync<IReadOnlyCollection<PaintLineDetails>>("api/paints/lines", "ricardo.smarzaro");
        var response3 = await TestingServer.GetAsync<IReadOnlyCollection<PaintLineDetails>>("api/paints/lines", "superuser");

        // Assert the responses
        response1.ShouldBeSuccess();
        response2.ShouldBeSuccess();
        response3.ShouldBeSuccess();

        // Verify all users get the same list
        var lines1 = response1.Data;
        var lines2 = response2.Data;
        var lines3 = response3.Data;
        
        lines1.ShouldNotBeNull();
        lines2.ShouldNotBeNull();
        lines3.ShouldNotBeNull();

        lines1.Count.ShouldBe(lines2.Count);
        lines2.Count.ShouldBe(lines3.Count);

        // Verify the same lines are returned
        var orderedLines1 = lines1.OrderBy(x => x.Id).ToList();
        var orderedLines2 = lines2.OrderBy(x => x.Id).ToList();
        var orderedLines3 = lines3.OrderBy(x => x.Id).ToList();

        for (int i = 0; i < orderedLines1.Count; i++)
        {
            orderedLines1[i].Id.ShouldBe(orderedLines2[i].Id);
            orderedLines2[i].Id.ShouldBe(orderedLines3[i].Id);
            orderedLines1[i].Name.ShouldBe(orderedLines2[i].Name);
            orderedLines2[i].Name.ShouldBe(orderedLines3[i].Name);
            orderedLines1[i].Brand.Id.ShouldBe(orderedLines2[i].Brand.Id);
            orderedLines2[i].Brand.Id.ShouldBe(orderedLines3[i].Brand.Id);
            orderedLines1[i].Brand.Name.ShouldBe(orderedLines2[i].Brand.Name);
            orderedLines2[i].Brand.Name.ShouldBe(orderedLines3[i].Brand.Name);
        }
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 