namespace PaintingProjectsManagement.Features.Inventory.Web.Tests;

[HumanFriendlyDisplayName]
public class Update_PaintBrand_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test brands
        var existingBrand = new PaintBrand("Existing Brand");
        var duplicateNameBrand = new PaintBrand("Duplicate Name Brand");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(existingBrand);
            await context.AddAsync(duplicateNameBrand);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedBrand = context.Set<PaintBrand>().FirstOrDefault(x => x.Name == "Existing Brand");
            savedBrand.ShouldNotBeNull();

            var savedDuplicateBrand = context.Set<PaintBrand>().FirstOrDefault(x => x.Name == "Duplicate Name Brand");
            savedDuplicateBrand.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("superuser", "admin", null);
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Update_Paint_Brand()
    {
        // Prepare
        var existingBrand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Existing Brand");
        existingBrand.ShouldNotBeNull("Brand should exist from seed");

        var updateRequest = new UpdatePaintBrand.Request
        {
            Id = existingBrand.Id,
            Name = "Updated Brand"
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/brands", updateRequest);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == existingBrand.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Brand");
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Update_Paint_Brand_That_Does_Not_Exist()
    {
        // Prepare
        var nonExistentId = Guid.NewGuid();
        var updateRequest = new UpdatePaintBrand.Request
        {
            Id = nonExistentId,
            Name = "Updated Brand"
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/brands", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database
        var brands = TestingServer.CreateContext().Set<PaintBrand>().Where(x => x.Name == "Updated Brand").ToList();
        brands.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Update_Paint_Brand_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var existingBrand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Existing Brand");
        existingBrand.ShouldNotBeNull("Brand should exist from seed");

        var updateRequest = new UpdatePaintBrand.Request
        {
            Id = existingBrand.Id,
            Name = name!
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/brands", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == existingBrand.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Brand");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Update_Paint_Brand_When_Name_Already_Exists()
    {
        // Prepare
        var existingBrand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Existing Brand");
        existingBrand.ShouldNotBeNull("Brand should exist from seed");

        var updateRequest = new UpdatePaintBrand.Request
        {
            Id = existingBrand.Id,
            Name = "Duplicate Name Brand" // This name already exists
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/brands", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Another brand with this name already exists.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == existingBrand.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Brand");
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Update_Paint_Brand_When_Name_Exceeds_MaxLength()
    {
        // Prepare
        var existingBrand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Existing Brand");
        existingBrand.ShouldNotBeNull("Brand should exist from seed");

        var updateRequest = new UpdatePaintBrand.Request
        {
            Id = existingBrand.Id,
            Name = new string('A', 101) // Exceeds max length of 100
        };

        // Act
        var response = await TestingServer.PutAsync("/api/paints/brands", updateRequest, "superuser");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database
        var unchangedEntity = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == existingBrand.Id);
        unchangedEntity.ShouldNotBeNull();
        unchangedEntity.Name.ShouldBe("Existing Brand");
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Can_Update_Paint_Brand()
    {
        // Prepare
        var existingBrand = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Name == "Existing Brand");
        existingBrand.ShouldNotBeNull("Brand should exist from seed");

        var updateRequest = new UpdatePaintBrand.Request
        {
            Id = existingBrand.Id,
            Name = "Updated Brand"
        };

        // Act
        var response = await TestingServer.PutAsync<PaintBrandDetails>("/api/paints/brands", updateRequest, "superuser");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(existingBrand.Id);
        result.Name.ShouldBe("Updated Brand");

        // Assert the database
        var updatedEntity = TestingServer.CreateContext().Set<PaintBrand>().FirstOrDefault(x => x.Id == existingBrand.Id);
        updatedEntity.ShouldNotBeNull();
        updatedEntity.Name.ShouldBe("Updated Brand");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
