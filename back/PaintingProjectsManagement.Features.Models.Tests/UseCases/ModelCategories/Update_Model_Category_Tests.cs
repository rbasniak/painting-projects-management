namespace PaintingProjectsManagement.Features.Models.Tests;

public class Update_Model_Category_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _categoryId;
    private static Guid _otherUserCategoryId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test model categories for update tests
        var category = new ModelCategory("rodrigo.basniak", "Original Category");
        var otherUserCategory = new ModelCategory("ricardo.smarzaro", "Other User Category");
        var existingCategory = new ModelCategory("rodrigo.basniak", "Existing Category");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(category);
            await context.AddAsync(otherUserCategory);
            await context.AddAsync(existingCategory);
            await context.SaveChangesAsync();
        }

        // Get the IDs for later use
        using (var context = TestingServer.CreateContext())
        {
            var savedCategory = context.Set<ModelCategory>().FirstOrDefault(x => x.Name == "Original Category");
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
    public async Task Non_Authenticated_User_Cannot_Update_Model_Category()
    {
        // Prepare
        var request = new UpdateModelCategory.Request
        {
            Id = _categoryId,
            Name = "Updated Category",
        };

        // Act
        var response = await TestingServer.PutAsync<ModelCategoryDetails>("api/models/categories", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - name should not be changed
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _categoryId);
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Original Category");
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Update_Non_Existent_Model_Category()
    {
        // Prepare
        var request = new UpdateModelCategory.Request
        {
            Id = Guid.NewGuid(), // Non-existent ID
            Name = "Updated Category",
        };

        // Act
        var response = await TestingServer.PutAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - no new category should be created
        var categories = TestingServer.CreateContext().Set<ModelCategory>().Where(x => x.Name == "Updated Category").ToList();
        categories.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Update_Model_Category_When_Name_Is_Empty()
    {
        // Prepare
        var request = new UpdateModelCategory.Request
        {
            Id = _categoryId,
            Name = "",
        };

        // Act
        var response = await TestingServer.PutAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database - name should not be changed
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _categoryId);
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Original Category");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Update_Model_Category_When_Name_Is_Null()
    {
        // Prepare
        var request = new UpdateModelCategory.Request
        {
            Id = _categoryId,
            Name = null!,
        };

        // Act
        var response = await TestingServer.PutAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database - name should not be changed
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _categoryId);
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Original Category");
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Update_Model_Category_When_Name_Exceeds_MaxLength()
    {
        // Prepare
        var request = new UpdateModelCategory.Request
        {
            Id = _categoryId,
            Name = new string('A', 101), // Exceeds max length of 100
        };

        // Act
        var response = await TestingServer.PutAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database - name should not be changed
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _categoryId);
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Original Category");
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Cannot_Update_Model_Category_When_Name_Already_Exists()
    {
        // Prepare
        var request = new UpdateModelCategory.Request
        {
            Id = _categoryId,
            Name = "Existing Category", // This name already exists
        };

        // Act
        var response = await TestingServer.PutAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "A model category with this name already exists.");

        // Assert the database - name should not be changed
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _categoryId);
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Original Category");
    }

    [Test, NotInParallel(Order = 8)]
    public async Task User_Cannot_Update_Model_Category_When_Name_Contains_Only_Whitespace()
    {
        // Prepare
        var request = new UpdateModelCategory.Request
        {
            Id = _categoryId,
            Name = "   ", // Only whitespace
        };

        // Act
        var response = await TestingServer.PutAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database - name should not be changed
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _categoryId);
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Original Category");
    }

    [Test, NotInParallel(Order = 9)]
    public async Task User_Can_Update_Model_Category_To_Same_Name_As_Another_User()
    {
        // Prepare
        var request = new UpdateModelCategory.Request
        {
            Id = _categoryId,
            Name = "Other User Category", // This name belongs to ricardo.smarzaro
        };

        // Act
        var response = await TestingServer.PutAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldBe(_categoryId);
        result.Name.ShouldBe("Other User Category");

        // Assert the database - both categories should exist with the same name but different tenants
        var categories = TestingServer.CreateContext().Set<ModelCategory>().Where(x => x.Name == "Other User Category").ToList();
        categories.Count.ShouldBe(2); // One from each user

        var rbCategory = categories.FirstOrDefault(x => x.TenantId == "RODRIGO.BASNIAK");
        var rsCategory = categories.FirstOrDefault(x => x.TenantId == "RICARDO.SMARZARO");

        rbCategory.ShouldNotBeNull();
        rsCategory.ShouldNotBeNull();
        rbCategory.Id.ShouldNotBe(rsCategory.Id);
    }

    [Test, NotInParallel(Order = 10)]
    public async Task User_Can_Update_Model_Category_With_Valid_Data()
    {
        // Prepare - first update the category back to a unique name
        var updateRequest = new UpdateModelCategory.Request
        {
            Id = _categoryId,
            Name = "Updated Category Name",
        };

        // Act
        var response = await TestingServer.PutAsync<ModelCategoryDetails>("api/models/categories", updateRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);

        result.Id.ShouldBe(_categoryId);
        result.Name.ShouldBe("Updated Category Name");

        // Assert the database
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _categoryId);
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Updated Category Name");
        category.TenantId.ShouldBe("RODRIGO.BASNIAK");
    }

    [Test, NotInParallel(Order = 11)]
    public async Task User_Cannot_Update_Model_Category_Of_Another_User()
    {
        // Prepare
        var request = new UpdateModelCategory.Request
        {
            Id = _otherUserCategoryId, // This belongs to ricardo.smarzaro
            Name = "Unauthorized Update",
        };

        // Act
        var response = await TestingServer.PutAsync<ModelCategoryDetails>("api/models/categories", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - name should not be changed
        var category = TestingServer.CreateContext().Set<ModelCategory>().FirstOrDefault(x => x.Id == _otherUserCategoryId);
        category.ShouldNotBeNull();
        category.Name.ShouldBe("Other User Category");
        category.TenantId.ShouldBe("RICARDO.SMARZARO");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
}