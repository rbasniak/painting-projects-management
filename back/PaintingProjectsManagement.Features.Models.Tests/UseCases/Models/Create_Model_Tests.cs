namespace PaintingProjectsManagement.Features.Models.Tests;

public class Create_Model_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _tenant1Category;
    private static Guid _tenant2Category;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create a test model category for the tests
        var tenant1TestCategory = new ModelCategory("rodrigo.basniak", "Tenant 1 Test Category");
        var tenant2TestCategory = new ModelCategory("ricardo.smarzaro", "Tenant 2 Other Tenant Test Category");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(tenant1TestCategory);
            await context.SaveChangesAsync();
            _tenant1Category = tenant1TestCategory.Id;

            await context.AddAsync(tenant2TestCategory);
            await context.SaveChangesAsync();
            _tenant2Category = tenant2TestCategory.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Create_Model()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Create_Model_When_Category_Is_Invalid()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = Guid.NewGuid(), // Invalid category ID
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "CategoryId references a non-existent record.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Create_Model_When_Category_Is_From_Another_User()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant2Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "CategoryId references a non-existent record.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Create_Model_When_Category_Is_From_Other_Tenant()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant2Category, // Category from different tenant
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "CategoryId references a non-existent record.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 6)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Create_Model_When_Artist_Is_Empty(string? artist)
    {
        var temp = TestingServer.CreateContext().Set<ModelCategory>().ToList();
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = artist!,
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Artist is required.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Cannot_Create_Model_When_Artist_Max_Length_Is_Exceeded()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = new string('A', 101), // Exceeds max length of 100
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Artist cannot exceed 50 characters.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 8)]
    public async Task User_Cannot_Create_Model_When_List_Of_Tags_Is_Null()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = null!,
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Tags is required.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 9)]
    public async Task User_Can_Create_Model_When_List_Of_Tags_Is_Empty()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model Empty Tags",
            Artist = "Test Artist",
            Tags = [],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test Model Empty Tags");
        result.Tags.ShouldBeEmpty();

        // Assert the database
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Name == "Test Model Empty Tags");
        model.ShouldNotBeNull();
        model.Tags.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 10)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Create_Model_When_Any_Of_The_Tags_In_The_List_Is_Empty(string? emptyTag)
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["validTag", emptyTag!, "anotherValidTag"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Each tag cannot be empty");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 11)]
    public async Task User_Cannot_Create_Model_When_Any_Of_The_Tags_In_The_List_Exceeds_Maximum_Length()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["validTag", new string('A', 26), "anotherValidTag"], // 26 characters exceeds max of 25
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Each tag cannot exceed 25 characters");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 12)]
    public async Task User_Cannot_Create_Model_When_Characters_Is_Null()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = null!,
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Characters is required.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 13)]
    public async Task User_Can_Create_Model_When_List_Of_Characters_Is_Empty()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model Empty Characters",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = [],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test Model Empty Characters");
        result.Characters.ShouldBeEmpty();

        // Assert the database
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Name == "Test Model Empty Characters");
        model.ShouldNotBeNull();
        model.Characters.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 14)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Create_Model_When_Any_Of_The_Characters_In_The_List_Is_Empty(string? emptyCharacter)
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["validCharacter", emptyCharacter!, "anotherValidCharacter"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Each character cannot be empty");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 15)]
    public async Task User_Cannot_Create_Model_When_Any_Of_The_Characters_In_The_List_Exceeds_Maximum_Length()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["validCharacter", new string('A', 51), "anotherValidCharacter"], // 51 characters exceeds max of 50
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Each character cannot exceed 50 characters");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 16)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Create_Model_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = name!,
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == name).ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 17)]
    public async Task User_Cannot_Create_Model_When_Max_Length_Of_Name_Is_Exceeded()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = new string('A', 101), // Exceeds max length of 100
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == new string('A', 101)).ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 18)]
    public async Task User_Cannot_Create_Model_When_BaseSize_Is_Invalid()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = (BaseSize)999, // Invalid enum value
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "BaseSize has an invalid value.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 19)]
    public async Task User_Cannot_Create_Model_When_FigureSize_Is_Invalid()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = (FigureSize)999, // Invalid enum value
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "FigureSize has an invalid value.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 20)]
    public async Task User_Cannot_Create_Model_When_The_Number_Of_Figures_Is_Invalid()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 0, // Invalid - must be greater than 0
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "NumberOfFigures must be greater than zero");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 21)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Create_Model_When_Franchise_Is_Empty(string? franchise)
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = franchise!,
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Franchise is required.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 22)]
    public async Task User_Cannot_Create_Model_When_Franchise_Max_Length_Is_Exceeded()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = new string('A', 101), // Exceeds max length of 100
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Franchise cannot exceed 75 characters.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 23)]
    public async Task User_Cannot_Create_Model_When_ModelType_Is_Invalid()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = (ModelType)999, // Invalid enum value
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Type has an invalid value.");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 24)]
    public async Task User_Cannot_Create_Model_When_The_Size_Is_Negative()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = -1 // Invalid - must be greater than or equal to 0
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "SizeInMb must be greater than or equal to zero");

        // Assert the database
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 25)]
    public async Task User_Can_Create_Model_When_Data_Is_Valid()
    {
        // Prepare
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1Category,
            Name = "Valid Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1", "character2"],
            Franchise = "Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 2,
            SizeInMb = 15
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Valid Test Model");
        result.Artist.ShouldBe("Test Artist");
        result.Tags.ShouldBe(["tag1", "tag2"]);
        result.Characters.ShouldBe(["character1", "character2"]);
        result.Franchise.ShouldBe("Test Franchise");
        //result.Type.ShouldBe(ModelType.Miniature);
        //result.BaseSize.ShouldBe(BaseSize.Medium);
        //result.FigureSize.ShouldBe(FigureSize.Normal);
        result.NumberOfFigures.ShouldBe(2);
        result.Size.ShouldBe(15);
        result.Category.Id.ShouldBe(_tenant1Category);

        // Assert the database
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Name == "Valid Test Model");
        model.ShouldNotBeNull();
        model.Artist.ShouldBe("Test Artist");
        model.Tags.ShouldBe(["tag1", "tag2"]);
        model.Characters.ShouldBe(["character1", "character2"]);
        model.Franchise.ShouldBe("Test Franchise");
        model.Type.ShouldBe(ModelType.Miniature);
        model.BaseSize.ShouldBe(BaseSize.Medium);
        model.FigureSize.ShouldBe(FigureSize.Normal);
        model.NumberOfFigures.ShouldBe(2);
        model.SizeInMb.ShouldBe(15);
        model.CategoryId.ShouldBe(_tenant1Category);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 