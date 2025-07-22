namespace PaintingProjectsManagement.Features.Paints.Tests;

public class Update_PaintBrand_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _brandId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create a test brand for the tests
        var testBrand = new PaintBrand(Guid.NewGuid(), "Test Brand for Update");
        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(testBrand);
            await context.SaveChangesAsync();
            _brandId = testBrand.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Update_PaintBrand()
    {
        // Prepare
        var request = new UpdatePaintBrand.Request
        {
            Id = _brandId,
            Name = "Updated Brand Name"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintBrandDetails>("api/paints/brands", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - name should not be changed
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldNotBeNull();
        brand.Name.ShouldBe("Test Brand for Update");
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Regular_User_Cannot_Update_PaintBrand()
    {
        // Prepare
        var request = new UpdatePaintBrand.Request
        {
            Id = _brandId,
            Name = "Updated Brand Name"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintBrandDetails>("api/paints/brands", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Forbidden);

        // Assert the database - name should not be changed
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldNotBeNull();
        brand.Name.ShouldBe("Test Brand for Update");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Superuser_Cannot_Update_PaintBrand_When_Id_Is_Invalid()
    {
        // Prepare
        var request = new UpdatePaintBrand.Request
        {
            Id = Guid.NewGuid(), // Invalid ID
            Name = "Updated Brand Name"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintBrandDetails>("api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - original brand should not be changed
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldNotBeNull();
        brand.Name.ShouldBe("Test Brand for Update");
    }

    [Test, NotInParallel(Order = 5)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task Superuser_Cannot_Update_PaintBrand_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var request = new UpdatePaintBrand.Request
        {
            Id = _brandId,
            Name = name!
        };

        // Act
        var response = await TestingServer.PutAsync<PaintBrandDetails>("api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database - name should not be changed
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldNotBeNull();
        brand.Name.ShouldBe("Test Brand for Update");
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Superuser_Cannot_Update_PaintBrand_When_Name_Already_Exists()
    {
        // Prepare - Create another brand first
        var existingBrand = new PaintBrand(Guid.NewGuid(), "Existing Brand Name");
        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(existingBrand);
            await context.SaveChangesAsync();
        }

        var request = new UpdatePaintBrand.Request
        {
            Id = _brandId,
            Name = "Existing Brand Name"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintBrandDetails>("api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Another brand with this name already exists.");

        // Assert the database - name should not be changed
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldNotBeNull();
        brand.Name.ShouldBe("Test Brand for Update");
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Superuser_Can_Update_PaintBrand_When_Data_Is_Valid()
    {
        // Prepare
        var request = new UpdatePaintBrand.Request
        {
            Id = _brandId,
            Name = "Updated Brand Name Valid"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintBrandDetails>("api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_brandId);
        result.Name.ShouldBe("Updated Brand Name Valid");

        // Assert the database
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldNotBeNull();
        brand.Name.ShouldBe("Updated Brand Name Valid");
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Superuser_Can_Update_PaintBrand_To_Same_Name()
    {
        // Prepare
        var request = new UpdatePaintBrand.Request
        {
            Id = _brandId,
            Name = "Updated Brand Name Valid" // Same name as previous test
        };

        // Act
        var response = await TestingServer.PutAsync<PaintBrandDetails>("api/paints/brands", request, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_brandId);
        result.Name.ShouldBe("Updated Brand Name Valid");

        // Assert the database
        var brand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == _brandId);
        brand.ShouldNotBeNull();
        brand.Name.ShouldBe("Updated Brand Name Valid");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 