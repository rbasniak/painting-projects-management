using PaintingProjectsManagement.Features.Models.Tests;

namespace PaintingProjectsManagement.Features.Models.Categories.Tests;

public class List_Model_Categories_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test model categories for list tests
        var category1 = new ModelCategory("rodrigo.basniak", "Category 1");
        var category2 = new ModelCategory("rodrigo.basniak", "Category 2");
        var otherUserCategory = new ModelCategory("ricardo.smarzaro", "Other User Category");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(category1);
            await context.AddAsync(category2);
            await context.AddAsync(otherUserCategory);
            await context.SaveChangesAsync();
        }

        // Assert the database
        using (var context = TestingServer.CreateContext())
        {
            var categories = context.Set<ModelCategory>().ToList();
            categories.Count.ShouldBe(3);
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task User_Can_List_Model_Categories()
    {
        // Act
        var response = await TestingServer.GetAsync<ModelCategoryDetails[]>("api/models/categories", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.ShouldNotBeNull();
        result.Length.ShouldBe(2); // Only rodrigo.basniak's categories

        var categoryNames = result.Select(x => x.Name).ToList();
        categoryNames.ShouldContain("Category 1");
        categoryNames.ShouldContain("Category 2");
        categoryNames.ShouldNotContain("Other User Category"); // Should not see other user's category
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Non_Authenticated_User_Cannot_List_Model_Categories()
    {
        // Act
        var response = await TestingServer.GetAsync<ModelCategoryDetails[]>("api/models/categories");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Different_Users_See_Their_Own_Categories()
    {
        // Act - rodrigo.basniak
        var rodrigoResponse = await TestingServer.GetAsync<ModelCategoryDetails[]>("api/models/categories", "rodrigo.basniak");

        // Act - ricardo.smarzaro
        var ricardoResponse = await TestingServer.GetAsync<ModelCategoryDetails[]>("api/models/categories", "ricardo.smarzaro");

        // Assert rodrigo's response
        rodrigoResponse.ShouldBeSuccess(out var rodrigoResult);
        rodrigoResult.Length.ShouldBe(2);
        rodrigoResult.Any(x => x.Name == "Category 1").ShouldBeTrue();
        rodrigoResult.Any(x => x.Name == "Category 2").ShouldBeTrue();
        rodrigoResult.Any(x => x.Name == "Other User Category").ShouldBeFalse();

        // Assert ricardo's response
        ricardoResponse.ShouldBeSuccess(out var ricardoResult);
        ricardoResult.Length.ShouldBe(1);
        ricardoResult.Any(x => x.Name == "Other User Category").ShouldBeTrue();
        ricardoResult.Any(x => x.Name == "Category 1").ShouldBeFalse();
        ricardoResult.Any(x => x.Name == "Category 2").ShouldBeFalse();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task List_Model_Categories_Returns_Empty_List_When_Database_Is_Empty()
    {
        // Clear the database
        using (var context = TestingServer.CreateContext())
        {
            context.Set<ModelCategory>().RemoveRange(context.Set<ModelCategory>());
            await context.SaveChangesAsync();
        }

        // Act
        var response = await TestingServer.GetAsync<ModelCategoryDetails[]>("api/models/categories", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.ShouldNotBeNull();
        result.Length.ShouldBe(0);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 