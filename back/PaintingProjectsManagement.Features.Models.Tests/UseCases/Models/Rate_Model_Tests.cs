namespace PaintingProjectsManagement.Features.Models.Tests;

public class Rate_Model_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

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
            _tenant1ModelId = tenant1Model.Id;

            await context.AddAsync(tenant2TestCategory);
            await context.AddAsync(tenant2Model);
            await context.SaveChangesAsync();
            _tenant2ModelId = tenant2Model.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Rate_Model()
    {
        // Prepare
        var request = new RateModel.Request
        {
            Id = _tenant1ModelId,
            Score = 3
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models/rate", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - score should remain unchanged
        using var context = TestingServer.CreateContext();
        var model = await context.Set<Model>().FirstAsync(x => x.Id == _tenant1ModelId);
        model.Score.Value.ShouldBe(0); // Default score
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Rate_Model_When_Model_Does_Not_Exist()
    {
        // Prepare
        var request = new RateModel.Request
        {
            Id = Guid.NewGuid(), // Non-existent model ID
            Score = 3
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models/rate", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - no changes should be made
        using var context = TestingServer.CreateContext();
        var model = await context.Set<Model>().FirstAsync(x => x.Id == _tenant1ModelId);
        model.Score.Value.ShouldBe(0); // Default score
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Rate_Model_From_Another_Tenant()
    {
        // Prepare
        var request = new RateModel.Request
        {
            Id = _tenant2ModelId, // Model from tenant 2
            Score = 3
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models/rate", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - score should remain unchanged
        using var context = TestingServer.CreateContext();
        var model = await context.Set<Model>().FirstAsync(x => x.Id == _tenant2ModelId);
        model.Score.Value.ShouldBe(0); // Default score
    }

    [Test, NotInParallel(Order = 5)]
    [Arguments(0)]
    [Arguments(6)]
    public async Task User_Cannot_Rate_Model_When_Score_Is_Out_Of_Range(int invalidScore)
    {
        // Prepare
        var request = new RateModel.Request
        {
            Id = _tenant1ModelId,
            Score = invalidScore
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models/rate", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Score must be a value between 1 and 5.");

        // Assert the database - score should remain unchanged
        using var context = TestingServer.CreateContext();
        var model = await context.Set<Model>().FirstAsync(x => x.Id == _tenant1ModelId);
        model.Score.Value.ShouldBe(0); // Default score
    }

    [Test, NotInParallel(Order = 6)]
    [Arguments(1)]
    [Arguments(2)]
    [Arguments(3)]
    [Arguments(4)]
    [Arguments(5)]
    public async Task User_Can_Rate_Model_When_Score_Is_Valid(int validScore)
    {
        // Prepare
        var request = new RateModel.Request
        {
            Id = _tenant1ModelId,
            Score = validScore
        };

        // Act
        var response = await TestingServer.PostAsync<ModelDetails>("api/models/rate", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Score.ShouldBe(validScore);

        // Assert the database
        using var context = TestingServer.CreateContext();
        var model = await context.Set<Model>().FirstAsync(x => x.Id == _tenant1ModelId);
        model.Score.Value.ShouldBe(validScore);
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Different_Users_Can_Rate_Their_Own_Models_Independently()
    {
        // User 1 rates their model
        var request1 = new RateModel.Request
        {
            Id = _tenant1ModelId,
            Score = 3
        };

        var response1 = await TestingServer.PostAsync<ModelDetails>("api/models/rate", request1, "rodrigo.basniak");
        response1.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Score.ShouldBe(3);

        // User 2 rates their model
        var request2 = new RateModel.Request
        {
            Id = _tenant2ModelId,
            Score = 5
        };

        var response2 = await TestingServer.PostAsync<ModelDetails>("api/models/rate", request2, "ricardo.smarzaro");
        response2.ShouldBeSuccess(out var result2);
        result2.ShouldNotBeNull();
        result2.Score.ShouldBe(5);

        // Assert the database - both models should have their respective scores
        using var context = TestingServer.CreateContext();
        var model1 = await context.Set<Model>().FirstAsync(x => x.Id == _tenant1ModelId);
        model1.Score.Value.ShouldBe(3);

        var model2 = await context.Set<Model>().FirstAsync(x => x.Id == _tenant2ModelId);
        model2.Score.Value.ShouldBe(5);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 