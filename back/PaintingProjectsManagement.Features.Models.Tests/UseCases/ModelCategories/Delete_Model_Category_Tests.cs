namespace PaintingProjectsManagement.Features.Models.Tests;

public class Delete_Model_Category_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _categoryId;
    private static Guid _otherUserCategoryId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test model categories for delete tests
        var category = new ModelCategory("rodrigo.basniak", "Category To Delete");
        var otherUserCategory = new ModelCategory("ricardo.smarzaro", "Other User Category");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(category);
            await context.AddAsync(otherUserCategory);
            await context.SaveChangesAsync();
        }

        // Get the IDs for later use
        using (var context = TestingServer.CreateContext())
        {
            var savedCategory = context.Set<ModelCategory>().FirstOrDefault(x => x.Name == "Category To Delete");
            var savedOtherUserCategory = context.Set<ModelCategory>().FirstOrDefault(x => x.Name == "Other User Category");
            
            savedCategory.ShouldNotBeNull();
            savedOtherUserCategory.ShouldNotBeNull();
            
            _categoryId = savedCategory.Id;
            _otherUserCategoryId = savedOtherUserCategory.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Delete_Model_Category()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/models/categories/{_categoryId}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - category should still exist
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _categoryId);
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Category To Delete");
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Delete_Non_Existent_Model_Category()
    {
        // Prepare
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"api/models/categories/{nonExistentId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - no categories should be deleted
        var categories = TestingServer.CreateContext().Set<ModelCategory>().ToList();
        categories.Count.ShouldBe(2); // Both original categories should still exist
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Delete_Model_Category_Of_Another_User()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/models/categories/{_otherUserCategoryId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - category should still exist
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _otherUserCategoryId);
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Other User Category");
        category.TenantId.ShouldBe("RICARDO.SMARZARO");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Can_Delete_Own_Model_Category()
    {
        // Act
        var response = await TestingServer.DeleteAsync($"api/models/categories/{_categoryId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - category should be deleted
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _categoryId);
        category.ShouldBeNull();

        // Assert that other user's category still exists
        var otherCategory = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _otherUserCategoryId);
        otherCategory.ShouldNotBeNull();
        otherCategory.Name.ShouldBe("Other User Category");
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Delete_Model_Category_With_Invalid_Guid_Format()
    {
        // Prepare
        var invalidGuid = "invalid-guid-format";

        // Act
        var response = await TestingServer.DeleteAsync($"api/models/categories/{invalidGuid}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest);

        // Assert the database - no categories should be deleted
        var categories = TestingServer.CreateContext().Set<ModelCategory>().ToList();
        categories.Count.ShouldBe(1); // Only the other user's category should exist
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Cannot_Delete_Model_Category_With_Empty_Guid()
    {
        // Prepare
        var emptyGuid = Guid.Empty;

        // Act
        var response = await TestingServer.DeleteAsync($"api/models/categories/{emptyGuid}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id is required.");

        // Assert the database - no categories should be deleted
        var categories = TestingServer.CreateContext().Set<ModelCategory>().ToList();
        categories.Count.ShouldBe(1); // Only the other user's category should exist
    }

    [Test, NotInParallel(Order = 8)]
    public async Task User_Can_Delete_Model_Category_And_Verify_Other_Users_Categories_Unaffected()
    {
        // Create another category for this user
        var anotherCategory = new ModelCategory("rodrigo.basniak", "Another Category To Delete");
        Guid anotherCategoryId;

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(anotherCategory);
            await context.SaveChangesAsync();
            anotherCategoryId = anotherCategory.Id;
        }

        // Act
        var response = await TestingServer.DeleteAsync($"api/models/categories/{anotherCategoryId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - only the specific category should be deleted
        var deletedCategory = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == anotherCategoryId);
        deletedCategory.ShouldBeNull();

        // Assert that other user's category still exists
        var otherCategory = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _otherUserCategoryId);
        otherCategory.ShouldNotBeNull();
        otherCategory.Name.ShouldBe("Other User Category");
        otherCategory.TenantId.ShouldBe("RICARDO.SMARZARO");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
}