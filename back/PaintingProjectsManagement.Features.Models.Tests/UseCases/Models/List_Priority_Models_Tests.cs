namespace PaintingProjectsManagement.Features.Models.Tests;

public class List_Priority_Models_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _tenant1PriorityModel1Id;
    private static Guid _tenant1PriorityModel2Id;
    private static Guid _tenant1PriorityModel3Id;
    private static Guid _tenant1NonPriorityModelId;
    private static Guid _tenant2PriorityModelId;
    private static Guid _tenant2NonPriorityModelId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test model categories for the tests
        var tenant1CategoryA = new ModelCategory("rodrigo.basniak", "Category A");
        var tenant1CategoryB = new ModelCategory("rodrigo.basniak", "Category B");
        var tenant2Category = new ModelCategory("ricardo.smarzaro", "Tenant 2 Category");

        // Create priority models (Score = 5) for tenant 1
        var tenant1PriorityModel1 = new Model(
            tenant: "rodrigo.basniak",
            name: "Priority Model 1",
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

        var tenant1PriorityModel2 = new Model(
            tenant: "rodrigo.basniak",
            name: "Priority Model 2",
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

        var tenant1PriorityModel3 = new Model(
            tenant: "rodrigo.basniak",
            name: "Priority Model 3",
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

        // Create non-priority model (Score != 5) for tenant 1
        var tenant1NonPriorityModel = new Model(
            tenant: "rodrigo.basniak",
            name: "Non-Priority Model",
            category: tenant1CategoryA,
            characters: ["Character4"],
            franchise: "Franchise1",
            type: ModelType.Figure,
            artist: "Artist4",
            tags: ["tag4"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 512);

        // Create priority model for tenant 2
        var tenant2PriorityModel = new Model(
            tenant: "ricardo.smarzaro",
            name: "Tenant 2 Priority Model",
            category: tenant2Category,
            characters: ["OtherCharacter"],
            franchise: "OtherFranchise",
            type: ModelType.Figure,
            artist: "OtherArtist",
            tags: ["otherTag"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 512);

        // Create non-priority model for tenant 2
        var tenant2NonPriorityModel = new Model(
            tenant: "ricardo.smarzaro",
            name: "Tenant 2 Non-Priority Model",
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
            await context.AddAsync(tenant1PriorityModel1);
            await context.AddAsync(tenant1PriorityModel2);
            await context.AddAsync(tenant1PriorityModel3);
            await context.AddAsync(tenant1NonPriorityModel);
            await context.AddAsync(tenant2PriorityModel);
            await context.AddAsync(tenant2NonPriorityModel);
            await context.SaveChangesAsync();

            // Set scores and priorities
            tenant1PriorityModel1.Rate(5);
            tenant1PriorityModel1.UpdatePriority(3);

            tenant1PriorityModel2.Rate(5);
            tenant1PriorityModel2.UpdatePriority(1);

            tenant1PriorityModel3.Rate(5);
            tenant1PriorityModel3.UpdatePriority(2);

            tenant1NonPriorityModel.Rate(3); // Not a priority model

            tenant2PriorityModel.Rate(5);
            tenant2PriorityModel.UpdatePriority(1);

            tenant2NonPriorityModel.Rate(2); // Not a priority model

            await context.SaveChangesAsync();

            // Store IDs for assertions
            _tenant1PriorityModel1Id = tenant1PriorityModel1.Id;
            _tenant1PriorityModel2Id = tenant1PriorityModel2.Id;
            _tenant1PriorityModel3Id = tenant1PriorityModel3.Id;
            _tenant1NonPriorityModelId = tenant1NonPriorityModel.Id;
            _tenant2PriorityModelId = tenant2PriorityModel.Id;
            _tenant2NonPriorityModelId = tenant2NonPriorityModel.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_Priority_Models()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models/prioritized");

        // Assert
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Authenticated_User_Can_List_Only_Their_Tenant_Priority_Models()
    {
        // Act
        var response = await TestingServer.GetAsync<List<ModelDetails>>("api/models/prioritized", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        // Verify all returned models belong to tenant 1 and have Score = 5
        result.ShouldAllBe(m => m.Id == _tenant1PriorityModel1Id || m.Id == _tenant1PriorityModel2Id || m.Id == _tenant1PriorityModel3Id);
        result.ShouldAllBe(m => m.Score == 5);

        // Verify non-priority models are not included
        result.ShouldNotContain(m => m.Id == _tenant1NonPriorityModelId);
        result.ShouldNotContain(m => m.Id == _tenant2PriorityModelId);
        result.ShouldNotContain(m => m.Id == _tenant2NonPriorityModelId);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Priority_Models_Are_Ordered_By_Priority_Descending_Then_Category_Name_Then_Model_Name()
    {
        // Act
        var response = await TestingServer.GetAsync<List<ModelDetails>>("api/models/prioritized", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        // Expected order based on:
        // 1. Priority descending (3, 2, 1)
        // 2. Category name (Category A comes before Category B)
        // 3. Model name (alphabetical within same category and priority)
        var expectedOrder = new[] { _tenant1PriorityModel1Id, _tenant1PriorityModel3Id, _tenant1PriorityModel2Id };
        var actualOrder = result.Select(m => m.Id).ToArray();
        actualOrder.ShouldBe(expectedOrder);

        // Verify priorities are correct
        result.ToArray()[0].Priority.ShouldBe(3); // Priority Model 1
        result.ToArray()[1].Priority.ShouldBe(2); // Priority Model 3 (Category B)
        result.ToArray()[2].Priority.ShouldBe(1); // Priority Model 2 (Category A, but lower priority)
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Priority_Model_Details_Are_Correctly_Mapped()
    {
        // Act
        var response = await TestingServer.GetAsync<List<ModelDetails>>("api/models/prioritized", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();

        // Find the first priority model to verify mapping
        var model = result.FirstOrDefault(m => m.Id == _tenant1PriorityModel1Id);
        model.ShouldNotBeNull();

        // Verify all properties are correctly mapped
        model.Name.ShouldBe("Priority Model 1");
        model.Franchise.ShouldBe("Franchise1");
        model.Characters.ShouldBe(new[] { "Character1" });
        model.Size.ShouldBe(512);
        model.Category.Name.ShouldBe("Category A");
        model.Type.ShouldBe(ModelType.Figure);
        model.Artist.ShouldBe("Artist1");
        model.Tags.ShouldBe(new[] { "tag1" });
        model.BaseSize.ShouldBe(BaseSize.Medium);
        model.FigureSize.ShouldBe(FigureSize.Normal);
        model.NumberOfFigures.ShouldBe(1);
        model.Priority.ShouldBe(3);
        model.Score.ShouldBe(5); // Priority models have Score = 5
        model.PictureUrl.ShouldBeNull(); // No picture URL set
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Different_Tenant_User_Sees_Only_Their_Priority_Models()
    {
        // Act
        var response = await TestingServer.GetAsync<List<ModelDetails>>("api/models/prioritized", "ricardo.smarzaro");

        // Assert
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        // Verify only tenant 2's priority model is returned
        result.First().Id.ShouldBe(_tenant2PriorityModelId);
        result.First().Name.ShouldBe("Tenant 2 Priority Model");
        result.First().Score.ShouldBe(5);
        result.First().Priority.ShouldBe(1);

        // Verify non-priority models are not included
        result.ShouldNotContain(m => m.Id == _tenant2NonPriorityModelId);
    }
     
    [Test, NotInParallel(Order = 8)]
    public async Task Models_With_Score_5_But_Zero_Priority_Are_Included_And_Ordered_Correctly()
    {
        // Create a model with Score = 5 but Priority = 0
        using (var context = TestingServer.CreateContext())
        {
            var category = await context.Set<ModelCategory>().FirstAsync(c => c.TenantId == "RODRIGO.BASNIAK");
            
            var zeroPriorityModel = new Model(
                tenant: "rodrigo.basniak",
                name: "Zero Priority Model",
                category: category,
                characters: ["ZeroCharacter"],
                franchise: "ZeroFranchise",
                type: ModelType.Figure,
                artist: "ZeroArtist",
                tags: ["zeroTag"],
                baseSize: BaseSize.Medium,
                figureSize: FigureSize.Normal,
                numberOfFigures: 1,
                sizeInMb: 512);

            await context.AddAsync(zeroPriorityModel);
            await context.SaveChangesAsync();

            zeroPriorityModel.Rate(5); // Score = 5
            zeroPriorityModel.UpdatePriority(0); // Priority = 0
            await context.SaveChangesAsync();
        }

        // Act
        var response = await TestingServer.GetAsync<List<ModelDetails>>("api/models/prioritized", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Count.ShouldBe(4); // Now 4 priority models including the zero priority one

        // The zero priority model should be last in the list (lowest priority)
        var zeroPriorityModelFromDb = result.FirstOrDefault(m => m.Name == "Zero Priority Model");
        zeroPriorityModelFromDb.ShouldNotBeNull();
        zeroPriorityModelFromDb.Score.ShouldBe(5);
        zeroPriorityModelFromDb.Priority.ShouldBe(0);

        // Verify it's the last in the list
        result.Last().Name.ShouldBe("Zero Priority Model");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 