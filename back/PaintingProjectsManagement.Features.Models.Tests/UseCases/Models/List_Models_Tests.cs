namespace PaintingProjectsManagement.Features.Models.Tests;

public class List_Models_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _tenant1Model1Id;
    private static Guid _tenant1Model2Id;
    private static Guid _tenant1Model3Id;
    private static Guid _tenant2ModelId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test categories for different tenants
        var tenant1Category1 = new ModelCategory("rodrigo.basniak", "Category A");
        var tenant1Category2 = new ModelCategory("rodrigo.basniak", "Category B");
        var tenant2Category = new ModelCategory("ricardo.smarzaro", "Other Tenant Category");

        // Create test models for tenant 1
        var tenant1Model1 = new Model(
            tenant: "rodrigo.basniak",
            name: "Model A1",
            category: tenant1Category1,
            characters: ["Character1"],
            franchise: "Franchise1",
            type: ModelType.Miniature,
            artist: "Artist1",
            tags: ["tag1"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 100);

        var tenant1Model2 = new Model(
            tenant: "rodrigo.basniak",
            name: "Model A2",
            category: tenant1Category1,
            characters: ["Character2"],
            franchise: "Franchise1",
            type: ModelType.Miniature,
            artist: "Artist1",
            tags: ["tag2"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 200);

        var tenant1Model3 = new Model(
            tenant: "rodrigo.basniak",
            name: "Model B1",
            category: tenant1Category2,
            characters: ["Character3"],
            franchise: "Franchise2",
            type: ModelType.Figure,
            artist: "Artist2",
            tags: ["tag3"],
            baseSize: BaseSize.Big,
            figureSize: FigureSize.Big,
            numberOfFigures: 2,
            sizeInMb: 300);

        // Create test model for tenant 2
        var tenant2Model = new Model(
            tenant: "ricardo.smarzaro",
            name: "Other Tenant Model",
            category: tenant2Category,
            characters: ["OtherCharacter"],
            franchise: "OtherFranchise",
            type: ModelType.Figure,
            artist: "OtherArtist",
            tags: ["otherTag"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 150);

        using (var context = TestingServer.CreateContext())
        {
            // Add categories first
            await context.AddAsync(tenant1Category1);
            await context.AddAsync(tenant1Category2);
            await context.AddAsync(tenant2Category);
            await context.SaveChangesAsync();

            // Add models
            await context.AddAsync(tenant1Model1);
            await context.AddAsync(tenant1Model2);
            await context.AddAsync(tenant1Model3);
            await context.AddAsync(tenant2Model);
            await context.SaveChangesAsync();

            _tenant1Model1Id = tenant1Model1.Id;
            _tenant1Model2Id = tenant1Model2.Id;
            _tenant1Model3Id = tenant1Model3.Id;
            _tenant2ModelId = tenant2Model.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_List_Models()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models");

        // Assert
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Authenticated_User_Can_List_Only_Their_Tenant_Models()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess();
        var models = response.Data;
        models.ShouldNotBeNull();
        models.Count.ShouldBe(3);

        // Verify all returned models belong to tenant 1
        models.ShouldAllBe(x => x.Id == _tenant1Model1Id || x.Id == _tenant1Model2Id || x.Id == _tenant1Model3Id);
        
        // Verify tenant 2's model is not included
        models.ShouldNotContain(x => x.Id == _tenant2ModelId);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Models_Are_Ordered_By_Category_Name_Then_By_Model_Name()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess();
        var models = response.Data;
        models.ShouldNotBeNull();
        models.Count.ShouldBe(3);

        // Verify ordering: Category A (alphabetically first) models should come before Category B models
        // Within Category A, models should be ordered by name
        var expectedOrder = new[] { _tenant1Model1Id, _tenant1Model2Id, _tenant1Model3Id };
        var actualOrder = models.Select(x => x.Id).ToArray();
        actualOrder.ShouldBe(expectedOrder);
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Model_Details_Are_Correctly_Mapped()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess();
        var models = response.Data;
        models.ShouldNotBeNull();

        // Find the first model to verify mapping
        var model = models.FirstOrDefault(x => x.Id == _tenant1Model1Id);
        model.ShouldNotBeNull();
        
        // Verify all properties are correctly mapped
        model.Name.ShouldBe("Model A1");
        model.Franchise.ShouldBe("Franchise1");
        model.Characters.ShouldBe(new[] { "Character1" });
        model.Size.ShouldBe(100);
        model.Category.Name.ShouldBe("Category A");
        model.Type.ShouldBe(ModelType.Miniature);
        model.Artist.ShouldBe("Artist1");
        model.Tags.ShouldBe(new[] { "tag1" });
        model.BaseSize.ShouldBe(BaseSize.Small);
        model.FigureSize.ShouldBe(FigureSize.Normal);
        model.NumberOfFigures.ShouldBe(1);
        model.MustHave.ShouldBeFalse(); // Default must-have flag
        model.Score.ShouldBe(0); // Default score
        model.CoverPicture.ShouldBeNull(); // No cover picture set
        model.Pictures.ShouldBeEmpty(); // No pictures set
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Different_Tenant_User_Sees_Only_Their_Models()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models", "ricardo.smarzaro");

        // Assert
        response.ShouldBeSuccess();
        var models = response.Data;
        models.ShouldNotBeNull();
        models.Count.ShouldBe(1);

        // Verify only tenant 2's model is returned
        models.First().Id.ShouldBe(_tenant2ModelId);
        models.First().Name.ShouldBe("Other Tenant Model");
        models.First().Category.Name.ShouldBe("Other Tenant Category");
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Model_Details_Include_CoverPicture_And_Pictures_Properties()
    {
        // Act
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models", "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess();
        var models = response.Data;
        models.ShouldNotBeNull();

        // Verify all models have the new properties
        foreach (var model in models)
        {
            // These properties should exist and be accessible
            model.CoverPicture.ShouldNotBeNull(); // Can be null, but property should exist
            model.Pictures.ShouldNotBeNull(); // Should be an empty array, not null
        }
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 