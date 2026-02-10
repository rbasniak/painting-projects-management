namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

[HumanFriendlyDisplayName]
public class MyPaints_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands, lines, colors
        var brand = new PaintBrand("Citadel");

        using (var context = TestingServer.CreateContext())
        {
            await context.Set<UserPaint>().ExecuteDeleteAsync();
            await context.Set<PaintColor>().ExecuteDeleteAsync();
            await context.Set<PaintLine>().ExecuteDeleteAsync();
            await context.Set<PaintBrand>().ExecuteDeleteAsync();
            await context.SaveChangesAsync();

            await context.AddAsync(brand);
            await context.SaveChangesAsync();

            var line = new PaintLine(brand, "Base");
            await context.AddAsync(line);
            await context.SaveChangesAsync();

            var color1 = new PaintColor(line, "Abaddon Black", "#000000", 12.0, PaintType.Opaque, "21-25");
            var color2 = new PaintColor(line, "Corax White", "#FFFFFF", 12.0, PaintType.Opaque, "21-52");
            var color3 = new PaintColor(line, "Mephiston Red", "#FF0000", 12.0, PaintType.Opaque, "21-37");

            await context.AddAsync(color1);
            await context.AddAsync(color2);
            await context.AddAsync(color3);
            await context.SaveChangesAsync();

            // Add one color to rodrigo's inventory
            var userPaint = new UserPaint("RODRIGO.BASNIAK", color1.Id);
            await context.AddAsync(userPaint);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var colors = context.Set<PaintColor>().ToList();
            colors.Count.ShouldBe(3);

            var userPaints = context.Set<UserPaint>().ToList();
            userPaints.Count.ShouldBe(1);
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_My_Paints()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<MyPaintDetails>>("/api/inventory/my-paints");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Can_List_Their_Own_Paints()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<MyPaintDetails>>("/api/inventory/my-paints", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(1);

        // Verify the paint belongs to the correct user
        var paint = response.Data.First();
        paint.Name.ShouldBe("Abaddon Black");
        paint.HexColor.ShouldBe("#000000");
        paint.BrandName.ShouldBe("Citadel");
        paint.LineName.ShouldBe("Base");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_See_Paints_From_Other_Users()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<MyPaintDetails>>("/api/inventory/my-paints", "ricardo.smarzaro");

        // Assert the response
        response.ShouldBeSuccess();
        response.Data.ShouldNotBeNull();
        response.Data.Count.ShouldBe(0); // ricardo has no paints
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Non_Authenticated_User_Cannot_Add_To_My_Paints()
    {
        // Prepare
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Name == "Corax White");
        color.ShouldNotBeNull();

        var request = new AddToMyPaints.Request
        {
            PaintColorIds = new List<Guid> { color.Id }
        };

        // Act
        var response = await TestingServer.PostAsync("/api/inventory/my-paints", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var userPaints = TestingServer.CreateContext().Set<UserPaint>().Where(x => x.PaintColorId == color.Id).ToList();
        userPaints.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Can_Add_Paint_To_My_Paints()
    {
        // Prepare
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Name == "Corax White");
        color.ShouldNotBeNull();

        var request = new AddToMyPaints.Request
        {
            PaintColorIds = new List<Guid> { color.Id }
        };

        // Act
        var response = await TestingServer.PostAsync("/api/inventory/my-paints", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database
        var userPaint = TestingServer.CreateContext().Set<UserPaint>().FirstOrDefault(x => x.Username == "RODRIGO.BASNIAK" && x.PaintColorId == color.Id);
        userPaint.ShouldNotBeNull();

        // Verify the list now contains 2 paints
        var listResponse = await TestingServer.GetAsync<IReadOnlyCollection<MyPaintDetails>>("/api/inventory/my-paints", "rodrigo.basniak");
        listResponse.ShouldBeSuccess();
        listResponse.Data.Count.ShouldBe(2);
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Can_Add_Multiple_Paints_At_Once()
    {
        // Prepare
        var color1 = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Name == "Mephiston Red");
        color1.ShouldNotBeNull();

        var request = new AddToMyPaints.Request
        {
            PaintColorIds = new List<Guid> { color1.Id }
        };

        // Act
        var response = await TestingServer.PostAsync("/api/inventory/my-paints", request, "ricardo.smarzaro");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database
        var userPaints = TestingServer.CreateContext().Set<UserPaint>().Where(x => x.Username == "RICARDO.SMARZARO").ToList();
        userPaints.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Adding_Duplicate_Paint_Does_Not_Create_Duplicate_Entry()
    {
        // Prepare
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Name == "Abaddon Black");
        color.ShouldNotBeNull();

        var request = new AddToMyPaints.Request
        {
            PaintColorIds = new List<Guid> { color.Id }
        };

        // Act - Try to add the same paint again
        var response = await TestingServer.PostAsync("/api/inventory/my-paints", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - should still have only one entry
        var userPaints = TestingServer.CreateContext().Set<UserPaint>().Where(x => x.Username == "RODRIGO.BASNIAK" && x.PaintColorId == color.Id).ToList();
        userPaints.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 9)]
    public async Task Non_Authenticated_User_Cannot_Remove_From_My_Paints()
    {
        // Prepare
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Name == "Abaddon Black");
        color.ShouldNotBeNull();

        // Act
        var response = await TestingServer.DeleteAsync($"/api/inventory/my-paints/{color.Id}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - should still exist
        var userPaint = TestingServer.CreateContext().Set<UserPaint>().FirstOrDefault(x => x.Username == "RODRIGO.BASNIAK" && x.PaintColorId == color.Id);
        userPaint.ShouldNotBeNull();
    }

    [Test, NotInParallel(Order = 10)]
    public async Task User_Can_Remove_Paint_From_My_Paints()
    {
        // Prepare
        var color = TestingServer.CreateContext().Set<PaintColor>().FirstOrDefault(x => x.Name == "Corax White");
        color.ShouldNotBeNull();

        // Act
        var response = await TestingServer.DeleteAsync($"/api/inventory/my-paints/{color.Id}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - should be removed
        var userPaint = TestingServer.CreateContext().Set<UserPaint>().FirstOrDefault(x => x.Username == "RODRIGO.BASNIAK" && x.PaintColorId == color.Id);
        userPaint.ShouldBeNull();

        // Verify the list now contains 1 paint
        var listResponse = await TestingServer.GetAsync<IReadOnlyCollection<MyPaintDetails>>("/api/inventory/my-paints", "rodrigo.basniak");
        listResponse.ShouldBeSuccess();
        listResponse.Data.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 11)]
    public async Task Removing_Non_Existent_Paint_Returns_NotFound()
    {
        // Prepare - Use a non-existent color ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"/api/inventory/my-paints/{nonExistentId}", "rodrigo.basniak");

        // Assert the response 
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "PaintColorId references a non-existent record.");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
