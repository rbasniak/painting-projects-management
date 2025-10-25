namespace PaintingProjectsManagement.Features.Models.Tests;

public class List_Priority_Models_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _tenant1MustHaveModel1Id;
    private static Guid _tenant1MustHaveModel2Id;
    private static Guid _tenant1MustHaveModel3Id;
    private static Guid _tenant1NonMustHaveModelId;
    private static Guid _tenant2MustHaveModelId;
    private static Guid _tenant2NonMustHaveModelId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test model categories for the tests
        var tenant1CategoryA = new ModelCategory("rodrigo.basniak", "Category A");
        var tenant1CategoryB = new ModelCategory("rodrigo.basniak", "Category B");
        var tenant2Category = new ModelCategory("ricardo.smarzaro", "Tenant 2 Category");

        // Create must-have models for tenant 1
        var tenant1MustHaveModel1 = new Model(
            tenant: "rodrigo.basniak",
            name: "Must Have Model 1",
            category: tenant1CategoryA,
            characters: ["Character1"],
            franchise: "Franchise1",
            type: ModelType.Figure,
            artist: "Artist1",
            tags: ["tag1"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 512);

        var tenant1MustHaveModel2 = new Model(
            tenant: "rodrigo.basniak",
            name: "Must Have Model 2",
            category: tenant1CategoryA,
            characters: ["Character2"],
            franchise: "Franchise1",
            type: ModelType.Figure,
            artist: "Artist2",
            tags: ["tag2"],
            baseSize: BaseSize.Big,
            figureSize: FigureSize.Big,
            numberOfFigures: 2,
            sizeInMb: 1024);

        var tenant1MustHaveModel3 = new Model(
            tenant: "rodrigo.basniak",
            name: "Must Have Model 3",
            category: tenant1CategoryB,
            characters: ["Character3"],
            franchise: "Franchise2",
            type: ModelType.Miniature,
            artist: "Artist3",
            tags: ["tag3"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 256);

        // Create non-must-have model for tenant 1
        var tenant1NonMustHaveModel = new Model(
            tenant: "rodrigo.basniak",
            name: "Non Must Have Model",
            category: tenant1CategoryA,
            characters: ["OtherCharacter1"],
            franchise: "OtherFranchise",
            type: ModelType.Vehicle,
            artist: "OtherArtist1",
            tags: ["otherTag1"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 768);

        // Create must-have model for tenant 2
        var tenant2MustHaveModel = new Model(
            tenant: "ricardo.smarzaro",
            name: "Tenant 2 Must Have Model",
            category: tenant2Category,
            characters: ["OtherCharacter1"],
            franchise: "OtherFranchise",
            type: ModelType.Figure,
            artist: "OtherArtist1",
            tags: ["otherTag1"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 512);

        // Create non-must-have model for tenant 2
        var tenant2NonMustHaveModel = new Model(
            tenant: "ricardo.smarzaro",
            name: "Tenant 2 Non Must Have Model",
            category: tenant2Category,
            characters: ["OtherCharacter2"],
            franchise: "OtherFranchise",
            type: ModelType.Miniature,
            artist: "OtherArtist2",
            tags: ["otherTag2"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 256);

        using (var context = TestingServer.CreateContext())
        {
            // Add categories first
            await context.AddAsync(tenant1CategoryA);
            await context.AddAsync(tenant1CategoryB);
            await context.AddAsync(tenant2Category);
            await context.SaveChangesAsync();

            // Add models
            await context.AddAsync(tenant1MustHaveModel1);
            await context.AddAsync(tenant1MustHaveModel2);
            await context.AddAsync(tenant1MustHaveModel3);
            await context.AddAsync(tenant1NonMustHaveModel);
            await context.AddAsync(tenant2MustHaveModel);
            await context.AddAsync(tenant2NonMustHaveModel);
            await context.SaveChangesAsync();

            // Set scores and must-have flags
            tenant1MustHaveModel1.Rate(5);
            tenant1MustHaveModel1.SetMustHave(true);

            tenant1MustHaveModel2.Rate(3);
            tenant1MustHaveModel2.SetMustHave(true);

            tenant1MustHaveModel3.Rate(4);
            tenant1MustHaveModel3.SetMustHave(true);

            tenant1NonMustHaveModel.Rate(3); // Not a must-have model
            tenant1NonMustHaveModel.SetMustHave(false);

            tenant2MustHaveModel.Rate(5);
            tenant2MustHaveModel.SetMustHave(true);

            tenant2NonMustHaveModel.Rate(2); // Not a must-have model
            tenant2NonMustHaveModel.SetMustHave(false);

            await context.SaveChangesAsync();

            // Store IDs for assertions
            _tenant1MustHaveModel1Id = tenant1MustHaveModel1.Id;
            _tenant1MustHaveModel2Id = tenant1MustHaveModel2.Id;
            _tenant1MustHaveModel3Id = tenant1MustHaveModel3.Id;
            _tenant1NonMustHaveModelId = tenant1NonMustHaveModel.Id;
            _tenant2MustHaveModelId = tenant2MustHaveModel.Id;
            _tenant2NonMustHaveModelId = tenant2NonMustHaveModel.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_Priority_Models()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models/must-have");

        // Assert
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Authenticated_User_Can_List_Only_Their_Tenant_Must_Have_Models()
    {
        // Act
        var response = await TestingServer.GetAsync<List<ModelDetails>>("api/models/must-have", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        // Verify all returned models belong to tenant 1 and have MustHave = true
        result.ShouldAllBe(x => x.Id == _tenant1MustHaveModel1Id || x.Id == _tenant1MustHaveModel2Id || x.Id == _tenant1MustHaveModel3Id);
        result.ShouldAllBe(x => x.MustHave == true);

        // Verify non-must-have models are not included
        result.ShouldNotContain(x => x.Id == _tenant1NonMustHaveModelId);
        result.ShouldNotContain(x => x.Id == _tenant2MustHaveModelId);
        result.ShouldNotContain(x => x.Id == _tenant2NonMustHaveModelId);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Must_Have_Models_Are_Ordered_By_Score_Descending_Then_Category_Name_Then_Model_Name()
    {
        // Act
        var response = await TestingServer.GetAsync<List<ModelDetails>>("api/models/must-have", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        // Expected order:
        // 1. Score descending (5, 4, 3)
        // 2. Category name (alphabetical within same score)
        // 3. Model name (alphabetical within same category and score)
        var expectedOrder = new[] { _tenant1MustHaveModel1Id, _tenant1MustHaveModel3Id, _tenant1MustHaveModel2Id };

        result.Select(x => x.Id).ShouldBe(expectedOrder);

        // Verify scores are in descending order
        result.ToArray()[0].Score.ShouldBe(5); // Must Have Model 1
        result.ToArray()[1].Score.ShouldBe(4); // Must Have Model 3 (Category B)
        result.ToArray()[2].Score.ShouldBe(3); // Must Have Model 2 (Category A, but lower score)
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Must_Have_Model_Details_Are_Correctly_Mapped()
    {
        // Act
        var response = await TestingServer.GetAsync<List<ModelDetails>>("api/models/must-have", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();

        // Find the first must-have model to verify mapping
        var model = result.FirstOrDefault(x => x.Id == _tenant1MustHaveModel1Id);
        model.ShouldNotBeNull();

        // Verify all properties are correctly mapped
        model.Id.ShouldBe(_tenant1MustHaveModel1Id);
        model.Name.ShouldBe("Must Have Model 1");
        model.Franchise.ShouldBe("Franchise1");
        model.Characters.ShouldBe(["Character1"]);
        model.Size.ShouldBe(512);
        model.Category.Id.ShouldNotBe(Guid.Empty);
        model.Category.Name.ShouldBe("Category A");
        // model.Type.ShouldBe(ModelType.Figure);
        model.Artist.ShouldBe("Artist1");
        model.Tags.ShouldBe(["tag1"]);
        // model.BaseSize.ShouldBe(BaseSize.Medium);
        // model.FigureSize.ShouldBe(FigureSize.Normal);
        model.NumberOfFigures.ShouldBe(1);
        model.Score.ShouldBe(5);
        model.MustHave.ShouldBe(true);
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Different_Tenant_User_Sees_Only_Their_Must_Have_Models()
    {
        // Act
        var response = await TestingServer.GetAsync<List<ModelDetails>>("api/models/must-have", "ricardo.smarzaro");

        // Assert
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        // Verify only tenant 2's must-have model is returned
        result.First().Id.ShouldBe(_tenant2MustHaveModelId);
        result.First().MustHave.ShouldBe(true);
        result.First().Score.ShouldBe(5);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 