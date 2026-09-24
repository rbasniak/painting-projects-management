namespace PaintingProjectsManagement.Features.Models.Tests;

/// <summary>
/// 📌 Proactive tests for MustHave bug fix and SizeInMb new field
/// Tests that model properties persist correctly through create/update operations
/// </summary>
[HumanFriendlyDisplayName]
public class Model_MustHave_And_SizeInMb_Persistence_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _tenant1CategoryId;
    private static Guid _modelWithMustHaveTrue;
    private static Guid _modelWithMustHaveFalse;
    private static Guid _modelWithSizeInMb;
    private static Guid _modelForUpdate;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test categories
        var tenant1Category = new ModelCategory("rodrigo.basniak", "Test Category");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(tenant1Category);
            await context.SaveChangesAsync();
            _tenant1CategoryId = tenant1Category.Id;
        }

        // Login with the user
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
    }

    #region MustHave Bug Fix Tests

    [Test, NotInParallel(Order = 2)]
    public async Task Creating_Model_With_MustHave_True_Persists_As_True()
    {
        // Prepare - Create via domain model with MustHave = true
        var model = new Model(
            tenant: "rodrigo.basniak",
            name: "Model With MustHave True",
            category: TestingServer.CreateContext().Set<ModelCategory>().First(x => x.Id == _tenant1CategoryId),
            characters: ["Character1"],
            franchise: "Franchise1",
            type: ModelType.Figure,
            artist: "Artist1",
            tags: ["tag1"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 100);
        
        model.SetMustHave(true);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(model);
            await context.SaveChangesAsync();
            _modelWithMustHaveTrue = model.Id;
        }

        // Assert - Retrieve from database and verify MustHave is true
        using (var context = TestingServer.CreateContext())
        {
            var retrieved = context.Set<Model>().FirstOrDefault(x => x.Id == _modelWithMustHaveTrue);
            retrieved.ShouldNotBeNull();
            retrieved.MustHave.ShouldBeTrue("MustHave should persist as true when set to true");
        }
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Creating_Model_With_MustHave_False_Persists_As_False()
    {
        // Prepare - Create via domain model (defaults to MustHave = false)
        var model = new Model(
            tenant: "rodrigo.basniak",
            name: "Model With MustHave False",
            category: TestingServer.CreateContext().Set<ModelCategory>().First(x => x.Id == _tenant1CategoryId),
            characters: ["Character2"],
            franchise: "Franchise2",
            type: ModelType.Miniature,
            artist: "Artist2",
            tags: ["tag2"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 50);

        // Explicitly verify it's false (default)
        model.MustHave.ShouldBeFalse("New models should default to MustHave = false");

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(model);
            await context.SaveChangesAsync();
            _modelWithMustHaveFalse = model.Id;
        }

        // Assert - Retrieve from database and verify MustHave is false
        using (var context = TestingServer.CreateContext())
        {
            var retrieved = context.Set<Model>().FirstOrDefault(x => x.Id == _modelWithMustHaveFalse);
            retrieved.ShouldNotBeNull();
            retrieved.MustHave.ShouldBeFalse("MustHave should persist as false when set to false");
        }
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Updating_Model_MustHave_From_True_To_False_Persists_Change()
    {
        // Prepare - Get the model with MustHave = true
        using (var context = TestingServer.CreateContext())
        {
            var model = context.Set<Model>().First(x => x.Id == _modelWithMustHaveTrue);
            model.MustHave.ShouldBeTrue("Pre-condition: model should have MustHave = true");
            
            // Act - Update to false
            model.SetMustHave(false);
            await context.SaveChangesAsync();
        }

        // Assert - Retrieve again and verify it's now false
        using (var context = TestingServer.CreateContext())
        {
            var retrieved = context.Set<Model>().FirstOrDefault(x => x.Id == _modelWithMustHaveTrue);
            retrieved.ShouldNotBeNull();
            retrieved.MustHave.ShouldBeFalse("MustHave should persist as false after update from true");
        }
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Updating_Model_MustHave_From_False_To_True_Persists_Change()
    {
        // Prepare - Get the model with MustHave = false
        using (var context = TestingServer.CreateContext())
        {
            var model = context.Set<Model>().First(x => x.Id == _modelWithMustHaveFalse);
            model.MustHave.ShouldBeFalse("Pre-condition: model should have MustHave = false");
            
            // Act - Update to true
            model.SetMustHave(true);
            await context.SaveChangesAsync();
        }

        // Assert - Retrieve again and verify it's now true
        using (var context = TestingServer.CreateContext())
        {
            var retrieved = context.Set<Model>().FirstOrDefault(x => x.Id == _modelWithMustHaveFalse);
            retrieved.ShouldNotBeNull();
            retrieved.MustHave.ShouldBeTrue("MustHave should persist as true after update from false");
        }
    }

    #endregion

    #region SizeInMb New Field Tests

    [Test, NotInParallel(Order = 6)]
    public async Task Creating_Model_With_SizeInMb_Value_Persists_Correctly()
    {
        // Prepare - Create model with specific SizeInMb value
        var model = new Model(
            tenant: "rodrigo.basniak",
            name: "Model With Size 2048MB",
            category: TestingServer.CreateContext().Set<ModelCategory>().First(x => x.Id == _tenant1CategoryId),
            characters: ["Character3"],
            franchise: "Franchise3",
            type: ModelType.Figure,
            artist: "Artist3",
            tags: ["tag3"],
            baseSize: BaseSize.Big,
            figureSize: FigureSize.Big,
            numberOfFigures: 3,
            sizeInMb: 2048);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(model);
            await context.SaveChangesAsync();
            _modelWithSizeInMb = model.Id;
        }

        // Assert - Retrieve and verify SizeInMb
        using (var context = TestingServer.CreateContext())
        {
            var retrieved = context.Set<Model>().FirstOrDefault(x => x.Id == _modelWithSizeInMb);
            retrieved.ShouldNotBeNull();
            retrieved.SizeInMb.ShouldBe(2048, "SizeInMb should persist with the correct value");
        }
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Creating_Model_With_Zero_SizeInMb_Is_Allowed()
    {
        // Prepare - Create model with SizeInMb = 0
        var model = new Model(
            tenant: "rodrigo.basniak",
            name: "Model With Zero Size",
            category: TestingServer.CreateContext().Set<ModelCategory>().First(x => x.Id == _tenant1CategoryId),
            characters: ["Character4"],
            franchise: "Franchise4",
            type: ModelType.Miniature,
            artist: "Artist4",
            tags: ["tag4"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 0);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(model);
            await context.SaveChangesAsync();
        }

        // Assert - Retrieve and verify SizeInMb is 0
        using (var context = TestingServer.CreateContext())
        {
            var retrieved = context.Set<Model>().FirstOrDefault(x => x.Id == model.Id);
            retrieved.ShouldNotBeNull();
            retrieved.SizeInMb.ShouldBe(0, "SizeInMb can be 0 (unknown or not applicable)");
        }
    }

    [Test, NotInParallel(Order = 8)]
    public async Task Updating_Model_SizeInMb_Persists_Change()
    {
        // Prepare - Create a model for updating
        var model = new Model(
            tenant: "rodrigo.basniak",
            name: "Model For Size Update",
            category: TestingServer.CreateContext().Set<ModelCategory>().First(x => x.Id == _tenant1CategoryId),
            characters: ["Character5"],
            franchise: "Franchise5",
            type: ModelType.Figure,
            artist: "Artist5",
            tags: ["tag5"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 100);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(model);
            await context.SaveChangesAsync();
            _modelForUpdate = model.Id;
        }

        // Act - Update the SizeInMb via UpdateDetails
        using (var context = TestingServer.CreateContext())
        {
            var modelToUpdate = context.Set<Model>().First(x => x.Id == _modelForUpdate);
            var category = context.Set<ModelCategory>().First(x => x.Id == _tenant1CategoryId);
            
            modelToUpdate.UpdateDetails(
                name: "Model For Size Update",
                category: category,
                characters: ["Character5"],
                artist: "Artist5",
                tags: ["tag5"],
                baseSize: BaseSize.Medium,
                figureSize: FigureSize.Normal,
                numberOfFigures: 1,
                franchise: "Franchise5",
                type: ModelType.Figure,
                sizeInMb: 512); // Change from 100 to 512
            
            await context.SaveChangesAsync();
        }

        // Assert - Retrieve and verify the updated SizeInMb
        using (var context = TestingServer.CreateContext())
        {
            var retrieved = context.Set<Model>().FirstOrDefault(x => x.Id == _modelForUpdate);
            retrieved.ShouldNotBeNull();
            retrieved.SizeInMb.ShouldBe(512, "Updated SizeInMb should persist correctly");
        }
    }

    [Test, NotInParallel(Order = 9)]
    public async Task SizeInMb_Is_Returned_In_Model_Details_Response_Via_API()
    {
        // Prepare - Create a model via API
        var request = new CreateModel.Request
        {
            CategoryId = _tenant1CategoryId,
            Name = "API Test Model With Size",
            Artist = "API Artist",
            Tags = ["api-tag"],
            Characters = ["API Character"],
            Franchise = "API Franchise",
            Type = ModelType.Figure,
            BaseSize = BaseSize.Medium,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 1536
        };

        // Act - Create via API
        var createResponse = await TestingServer.PostAsync<ModelDetails>("models", request, "rodrigo.basniak");

        // Assert - Verify response includes SizeInMb
        createResponse.ShouldBeSuccess(out var createdModel);
        createdModel.ShouldNotBeNull();
        createdModel.Size.ShouldBe(1536, "API response should include SizeInMb as Size property");

        // Assert - Get the model and verify again
        var getResponse = await TestingServer.GetAsync<ModelDetails>($"models/{createdModel.Id}", "rodrigo.basniak");
        getResponse.ShouldBeSuccess(out var retrievedModel);
        retrievedModel.ShouldNotBeNull();
        retrievedModel.Size.ShouldBe(1536, "GET endpoint should return SizeInMb in response");
    }

    [Test, NotInParallel(Order = 10)]
    public async Task Updating_Model_Via_API_Persists_SizeInMb_Change()
    {
        // Prepare - Create a model first
        var createRequest = new CreateModel.Request
        {
            CategoryId = _tenant1CategoryId,
            Name = "Model To Update Size Via API",
            Artist = "Update Artist",
            Tags = ["update-tag"],
            Characters = ["Update Character"],
            Franchise = "Update Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Small,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 256
        };

        var createResponse = await TestingServer.PostAsync<ModelDetails>("models", createRequest, "rodrigo.basniak");
        createResponse.ShouldBeSuccess(out var createdModel);
        createdModel.Size.ShouldBe(256, "Initial size should be 256");

        // Act - Update the model with new SizeInMb
        var updateRequest = new UpdateModel.Request
        {
            Id = createdModel.Id,
            CategoryId = _tenant1CategoryId,
            Name = "Model To Update Size Via API",
            Artist = "Update Artist",
            Tags = ["update-tag"],
            Characters = ["Update Character"],
            Franchise = "Update Franchise",
            Type = ModelType.Miniature,
            BaseSize = BaseSize.Small,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 1,
            SizeInMb = 1024
        };

        var updateResponse = await TestingServer.PutAsync<ModelDetails>("models", updateRequest, "rodrigo.basniak");

        // Assert - Verify updated response
        updateResponse.ShouldBeSuccess(out var updatedModel);
        updatedModel.ShouldNotBeNull();
        updatedModel.Size.ShouldBe(1024, "Updated size should be 1024");

        // Assert - Verify in database
        using (var context = TestingServer.CreateContext())
        {
            var dbModel = context.Set<Model>().FirstOrDefault(x => x.Id == createdModel.Id);
            dbModel.ShouldNotBeNull();
            dbModel.SizeInMb.ShouldBe(1024, "Database should reflect the updated SizeInMb");
        }
    }

    #endregion

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
