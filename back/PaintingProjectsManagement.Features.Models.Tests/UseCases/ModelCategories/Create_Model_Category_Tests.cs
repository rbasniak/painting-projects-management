using PaintingProjectsManagement.Features.Models.Tests;

namespace PaintingProjectsManagement.Features.Models.Categories.Tests;

public class Create_Model_Category_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create a test model category for duplicate name validation tests
        var existingCategory = new ModelCategory("rodrigo.basniak", "Existing Category");

        using (var context = TestingServer.CreateContext())
        {
            var connectionString = context.Database.GetConnectionString();

            await context.AddAsync(existingCategory);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var savedCategory = context.Set<ModelCategory>().FirstOrDefault(x => x.Name == "Existing Category");
            savedCategory.ShouldNotBeNull();
        }

        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_Model_Category()
    {
        // Prepare
        var request = new CreateModelCategory.Request
        {
            Name = "Test Category",
        };

        // Act
        var response = await TestingServer.PostAsync<ModelCategoryDetails>("api/models/categories", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var categories = TestingServer.CreateContext().Set<ModelCategory>().Where(x => x.Name == "Test Category").ToList();
        categories.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 3)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Create_Model_Category_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var request = new CreateModelCategory.Request
        {
            Name = name!,
        };

        // Act
        var response = await TestingServer.PostAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var categories = TestingServer.CreateContext().Set<ModelCategory>().Where(x => x.Name == name).ToList();
        categories.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Create_Model_Category_When_Name_Exceeds_MaxLength()
    {
        // Prepare
        var request = new CreateModelCategory.Request
        {
            Name = new string('A', 101), // Exceeds max length of 100
        };

        // Act
        var response = await TestingServer.PostAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database
        var categories = TestingServer.CreateContext().Set<ModelCategory>().Where(x => x.Name.Length > 100).ToList();
        categories.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Create_Model_Category_When_Name_Already_Exists()
    {
        // Prepare
        var request = new CreateModelCategory.Request
        {
            Name = "Existing Category", // This name was created in Seed test
        };

        // Act
        var response = await TestingServer.PostAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A model category with this name already exists.");

        // Assert the database
        var categories = TestingServer.CreateContext().Set<ModelCategory>().Where(x => x.Name == "Existing Category").ToList();
        categories.Count.ShouldBe(1); // Only the original one from Seed
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Can_Create_Model_Category_With_Same_Name_As_Another_User()
    {
        // Prepare
        var request = new CreateModelCategory.Request
        {
            Name = "Existing Category", // This name was created by rodrigo.basniak in Seed test
        };

        // Act
        var response = await TestingServer.PostAsync<ModelCategoryDetails>("api/models/categories", request, "ricardo.smarzaro");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Existing Category");

        // Assert the database - should have two categories with the same name but different users
        var categories = TestingServer.CreateContext().Set<ModelCategory>().Where(x => x.Name == "Existing Category").ToList();
        categories.Count.ShouldBe(2); // One from rodrigo.basniak (Seed) and one from ricardo.smarzaro

        var rbCategory = categories.FirstOrDefault(x => x.TenantId == "RODRIGO.BASNIAK" && x.Name == "Existing Category");
        var rsCategory = categories.FirstOrDefault(x => x.TenantId == "RICARDO.SMARZARO" && x.Name == "Existing Category");

        rbCategory.ShouldNotBeNull();
        rbCategory.Id.ShouldNotBe(rsCategory.Id);

        rsCategory.ShouldNotBeNull();
    }

    /// <summary>
    /// The user should be able to create a new model category with valid data
    /// </summary>
    [Test, NotInParallel(Order = 8)]
    public async Task User_Can_Create_Model_Category()
    {
        // Prepare
        var request = new CreateModelCategory.Request
        {
            Name = "Test Category",
        };

        // Act
        var response = await TestingServer.PostAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("Test Category");

        // Assert the database
        var entity = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == result.Id);

        entity.ShouldNotBeNull();
        entity.Id.ShouldBe(result.Id);
        entity.Name.ShouldBe("Test Category");
        entity.TenantId.ShouldBe("RODRIGO.BASNIAK");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
}