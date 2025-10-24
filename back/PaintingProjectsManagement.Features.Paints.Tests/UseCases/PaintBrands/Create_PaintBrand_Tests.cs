namespace PaintingProjectsManagement.Features.Paints.Brands.Tests;

public class Create_PaintBrand_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_PaintBrand()
    {
        // Prepare
        var request = new CreatePaintBrand.Request
        {
            Name = "Test Brand"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintBrandDetails>("api/paints/brands", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var brands = TestingServer.CreateContext().Set<PaintBrand>().Where(x => x.Name == "Test Brand").ToList();
        brands.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Cannot_Create_PaintBrand()
    {
        // Prepare
        var request = new CreatePaintBrand.Request
        {
            Name = "Test Brand"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintBrandDetails>("api/paints/brands", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Forbidden);

        // Assert the database
        var brands = TestingServer.CreateContext().Set<PaintBrand>().Where(x => x.Name == "Test Brand").ToList();
        brands.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task Superuser_Cannot_Create_PaintBrand_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var request = new CreatePaintBrand.Request
        {
            Name = name!
        };

        // Act
        var response = await TestingServer.PostAsync<PaintBrandDetails>("api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var brands = TestingServer.CreateContext().Set<PaintBrand>().Where(x => x.Name == name).ToList();
        brands.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Superuser_Cannot_Create_PaintBrand_When_Name_Already_Exists()
    {
        // Prepare - Create a brand first
        var existingBrand = new PaintBrand("Existing Brand");
        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(existingBrand);
            await context.SaveChangesAsync();
        }

        var request = new CreatePaintBrand.Request
        {
            Name = "Existing Brand"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintBrandDetails>("api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A brand with this name already exists.");

        // Assert the database - should still have only one brand with this name
        var brands = TestingServer.CreateContext().Set<PaintBrand>().Where(x => x.Name == "Existing Brand").ToList();
        brands.Count.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Superuser_Can_Create_PaintBrand_When_Data_Is_Valid()
    {
        // Prepare
        var request = new CreatePaintBrand.Request
        {
            Name = "Test Brand Valid"
        };

        // Act
        var response = await TestingServer.PostAsync<PaintBrandDetails>("api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Test Brand Valid");

        // Assert the database
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Test Brand Valid");
        brand.ShouldNotBeNull();
        brand.Id.ShouldBe(result.Id);
        brand.Name.ShouldBe("Test Brand Valid");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 