namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

public class Create_PaintBrand_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create an existing brand for duplicate name validation tests
        var existingBrand = new PaintBrand("Existing Brand");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(existingBrand);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedBrand = context.Set<PaintBrand>().FirstOrDefault(x => x.Name == "Existing Brand");
            savedBrand.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_Paint_Brand()
    {
        // Prepare
        var request = new CreatePaintBrand.Request
        {
            Name = "Test Brand"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintBrandDetails>("/api/paints/brands", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var brands = TestingServer.CreateContext().Set<PaintBrand>().Where(x => x.Name == "Test Brand").ToList();
        brands.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 3)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Create_Paint_Brand_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var request = new CreatePaintBrand.Request
        {
            Name = name!
        };

        // Act
        var response = await TestingServer.PostAsync<PaintBrandDetails>("/api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var brands = TestingServer.CreateContext().Set<PaintBrand>().Where(x => x.Name == name).ToList();
        brands.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Create_Paint_Brand_When_Name_Exceeds_MaxLength()
    {
        // Prepare
        var request = new CreatePaintBrand.Request
        {
            Name = new string('A', 101) // Exceeds max length of 100
        };

        // Act
        var response = await TestingServer.PostAsync<PaintBrandDetails>("/api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database
        var brands = TestingServer.CreateContext().Set<PaintBrand>().Where(x => x.Name == new string('A', 101)).ToList();
        brands.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Create_Paint_Brand_When_Name_Already_Exists()
    {
        // Prepare
        var request = new CreatePaintBrand.Request
        {
            Name = "Existing Brand"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintBrandDetails>("/api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A brand with this name already exists.");

        // Assert the database - should still have only one brand with this name
        var brands = TestingServer.CreateContext().Set<PaintBrand>().Where(x => x.Name == "Existing Brand").ToList();
        brands.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Can_Create_Paint_Brand()
    {
        // Prepare
        var request = new CreatePaintBrand.Request
        {
            Name = "New Brand"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintBrandDetails>("/api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("New Brand");

        // Assert the database
        var entity = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == result.Id);

        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(result.Id);
        entity.Name.ShouldBe("New Brand");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
