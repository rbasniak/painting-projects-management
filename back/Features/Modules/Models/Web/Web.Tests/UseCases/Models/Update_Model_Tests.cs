namespace PaintingProjectsManagement.Features.Models.Tests;

[HumanFriendlyDisplayName]
public class Update_Model_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _tenant1CategoryId;
    private static Guid _tenant2CategoryId;
    private static Guid _tenant1ModelId;
    private static Guid _tenant2ModelId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create a test model category for the tests
        var tenant1TestCategory = new ModelCategory("rodrigo.basniak", "Tenant 1 Test Category");
        var tenant2TestCategory = new ModelCategory("ricardo.smarzaro", "Tenant 2 Other Test Category");

        var tenant1Model = new Model(
            tenant: "rodrigo.basniak", 
            name: "Model 1", 
            category: tenant1TestCategory, 
            characters: ["Character1", "Character2"], 
            franchise: "Franchise1", 
            type: ModelType.Figure,
            artist: "Artist1",
            tags: ["tag1", "tag2"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 2,
            sizeInMb: 512);

        var tenant2Model = new Model(
            tenant: "ricardo.smarzaro",
            name: "Model 2",
            category: tenant2TestCategory,
            characters: ["Character1"],
            franchise: "Franchise2",
            type: ModelType.Figure,
            artist: "Artist2",
            tags: ["tag1"],
            baseSize: BaseSize.Big,
            figureSize: FigureSize.Big,
            numberOfFigures: 1, 
            sizeInMb: 1034); 

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(tenant1TestCategory);
            await context.AddAsync(tenant1Model);
            await context.SaveChangesAsync();
            _tenant1CategoryId = tenant1TestCategory.Id;
            _tenant1ModelId = tenant1Model.Id;

            await context.AddAsync(tenant2TestCategory);
            await context.AddAsync(tenant2Model);
            await context.SaveChangesAsync();
            _tenant2CategoryId = tenant2TestCategory.Id;
            _tenant2ModelId = tenant2Model.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Update_Model()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Update_Model_When_Id_Is_From_Other_Tenant()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant2ModelId, // Model from different tenant
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant2ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 2"); // Original name should remain
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Update_Model_When_Id_Is_Invalid()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = Guid.NewGuid(), // Invalid model ID
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - no model should be created
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Name == "Updated Test Model").ToList();
        models.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Update_Model_When_Category_Is_Invalid()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = Guid.NewGuid(), // Invalid category ID
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "CategoryId references a non-existent record.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Update_Model_When_Category_Is_From_Other_Tenant()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant2CategoryId, // Category from different tenant
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "CategoryId references a non-existent record.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 7)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Update_Model_When_Artist_Is_Empty(string? artist)
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Artist is required.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 8)]
    public async Task User_Cannot_Update_Model_When_Artist_Max_Length_Is_Exceeded()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
            Artist = new string('A', 51), // Exceeds max length of 50
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Artist cannot exceed 50 characters.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 9)]
    public async Task User_Cannot_Update_Model_When_List_Of_Tags_Is_Null()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Tags is required.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 10)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Update_Model_When_Any_Of_The_Tags_In_The_List_Is_Empty(string? emptyTag)
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Each tag cannot be empty");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 11)]
    public async Task User_Cannot_Update_Model_When_Any_Of_The_Tags_In_The_List_Exceeds_Maximum_Length()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Each tag cannot exceed 25 characters");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 12)]
    public async Task User_Cannot_Update_Model_When_Characters_Is_Null()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Characters is required.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 13)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Update_Model_When_Any_Of_The_Characters_In_The_List_Is_Empty(string? emptyCharacter)
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Each character cannot be empty");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 14)]
    public async Task User_Cannot_Update_Model_When_Any_Of_The_Characters_In_The_List_Exceeds_Maximum_Length()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Each character cannot exceed 50 characters");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 15)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Update_Model_When_Name_Is_Empty(string? name)
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name is required.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 16)]
    public async Task User_Cannot_Update_Model_When_Max_Length_Of_Name_Is_Exceeded()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Name cannot exceed 100 characters.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 17)]
    public async Task User_Cannot_Update_Model_When_BaseSize_Is_Invalid()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "BaseSize has an invalid value.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 18)]
    public async Task User_Cannot_Update_Model_When_FigureSize_Is_Invalid()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "FigureSize has an invalid value.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 19)]
    public async Task User_Cannot_Update_Model_When_The_Number_Of_Figures_Is_Invalid()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "NumberOfFigures must be greater than zero");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 20)]
    [Arguments("")]
    [Arguments(null)]
    [Arguments("   ")]
    public async Task User_Cannot_Update_Model_When_Franchise_Is_Empty(string? franchise)
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Franchise is required.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 21)]
    public async Task User_Cannot_Update_Model_When_Franchise_Max_Length_Is_Exceeded()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
            Artist = "Test Artist",
            Tags = ["tag1", "tag2"],
            Characters = ["character1"],
            Franchise = new string('A', 76), // Exceeds max length of 75
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 10
        };

        // Act
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Franchise cannot exceed 75 characters.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 22)]
    public async Task User_Cannot_Update_Model_When_Type_Is_Invalid()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Type has an invalid value.");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 23)]
    public async Task User_Cannot_Update_Model_When_The_Size_Is_Negative()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "SizeInMb must be greater than or equal to zero");

        // Assert the database - model should not be updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Model 1"); // Original name should remain
    }

    [Test, NotInParallel(Order = 24)]
    public async Task User_Can_Update_Model_When_List_Of_Characters_Is_Empty()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model Empty Characters",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Updated Test Model Empty Characters");
        result.Characters.ShouldBeEmpty();

        // Assert the database
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Updated Test Model Empty Characters");
        model.Characters.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 25)]
    public async Task User_Can_Update_Model_When_List_Of_Tags_Is_Empty()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Updated Test Model Empty Tags",
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
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Updated Test Model Empty Tags");
        result.Tags.ShouldBeEmpty();

        // Assert the database
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Updated Test Model Empty Tags");
        model.Tags.ShouldBeEmpty();
    }

    [Test, NotInParallel(Order = 26)]
    public async Task User_Can_Update_Model_When_Data_Is_Valid()
    {
        // Prepare
        var request = new UpdateModel.Request
        {
            Id = _tenant1ModelId,
            CategoryId = _tenant1CategoryId,
            Name = "Valid Updated Test Model",
            Artist = "Updated Test Artist",
            Tags = ["updatedTag1", "updatedTag2"],
            Characters = ["updatedCharacter1", "updatedCharacter2"],
            Franchise = "Updated Test Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Big,
            FigureSize = FigureSize.Big,
            NumberOfFigures = 3,
            SizeInMb = 25
        };

        // Act
        var response = await TestingServer.PutAsync<ModelDetails>("api/models", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Valid Updated Test Model");
        result.Artist.ShouldBe("Updated Test Artist");
        result.Tags.ShouldBe(["updatedTag1", "updatedTag2"]);
        result.Characters.ShouldBe(["updatedCharacter1", "updatedCharacter2"]);
        result.Franchise.ShouldBe("Updated Test Franchise");
        //result.Type.ShouldBe(ModelType.Miniature);
        //result.BaseSize.ShouldBe(BaseSize.Big);
        //result.FigureSize.ShouldBe(FigureSize.Big);
        result.NumberOfFigures.ShouldBe(3);
        result.Size.ShouldBe(25);
        result.Category.Id.ShouldBe(_tenant1CategoryId);

        // Assert the database
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.Name.ShouldBe("Valid Updated Test Model");
        model.Artist.ShouldBe("Updated Test Artist");
        model.Tags.ShouldBe(["updatedTag1", "updatedTag2"]);
        model.Characters.ShouldBe(["updatedCharacter1", "updatedCharacter2"]);
        model.Franchise.ShouldBe("Updated Test Franchise");
        model.Type.ShouldBe(ModelType.Miniature);
        model.BaseSize.ShouldBe(BaseSize.Big);
        model.FigureSize.ShouldBe(FigureSize.Big);
        model.NumberOfFigures.ShouldBe(3);
        model.SizeInMb.ShouldBe(25);
        model.CategoryId.ShouldBe(_tenant1CategoryId);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 