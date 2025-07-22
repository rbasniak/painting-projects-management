using PaintingProjectsManagement.Features.Models.Tests;
using System.Net;
using Shouldly;

namespace PaintingProjectsManagement.Features.Models.Tests;

public class Prioritize_Models_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _tenant1Model1Id;
    private static Guid _tenant1Model2Id;
    private static Guid _tenant1Model3Id;
    private static Guid _tenant1Model4Id;
    private static Guid _tenant1ModelNotReadyId;
    private static Guid _tenant2Model1Id;
    private static Guid _tenant2Model2Id;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test model categories for the tests
        var tenant1TestCategory = new ModelCategory("rodrigo.basniak", "Tenant 1 Test Category");
        var tenant2TestCategory = new ModelCategory("ricardo.smarzaro", "Tenant 2 Other Test Category");

        // Create models with Score = 5 (eligible for prioritization)
        var tenant1Model1Ready = new Model(
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

        var tenant1Model2Ready = new Model(
            tenant: "rodrigo.basniak", 
            name: "Model 2", 
            category: tenant1TestCategory, 
            characters: ["Character3"], 
            franchise: "Franchise1", 
            type: ModelType.Figure,
            artist: "Artist2",
            tags: ["tag3"],
            baseSize: BaseSize.Big,
            figureSize: FigureSize.Big,
            numberOfFigures: 1,
            sizeInMb: 1024);

        var tenant1Model3Ready = new Model(
            tenant: "rodrigo.basniak", 
            name: "Model 3", 
            category: tenant1TestCategory, 
            characters: ["Character4"], 
            franchise: "Franchise2", 
            type: ModelType.Miniature,
            artist: "Artist3",
            tags: ["tag4"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 256);

        var tenant1Model4Ready = new Model(
            tenant: "rodrigo.basniak",
            name: "Model 4",
            category: tenant1TestCategory,
            characters: ["Character4"],
            franchise: "Franchise2",
            type: ModelType.Miniature,
            artist: "Artist3",
            tags: ["tag4"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 256);

        var tenant1ModelNotReady = new Model(
            tenant: "rodrigo.basniak",
            name: "Model 3 Not Ready",
            category: tenant1TestCategory,
            characters: ["Character4"],
            franchise: "Franchise2",
            type: ModelType.Miniature,
            artist: "Artist3",
            tags: ["tag4"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 256);

        var tenant2Model1Ready = new Model(
            tenant: "ricardo.smarzaro",
            name: "Model 4",
            category: tenant2TestCategory,
            characters: ["Character5"],
            franchise: "Franchise3",
            type: ModelType.Figure,
            artist: "Artist4",
            tags: ["tag5"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1, 
            sizeInMb: 768);

        var tenant2Model2Ready = new Model(
            tenant: "ricardo.smarzaro",
            name: "Model 5",
            category: tenant2TestCategory,
            characters: ["Character6"],
            franchise: "Franchise3",
            type: ModelType.Miniature,
            artist: "Artist5",
            tags: ["tag6"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1, 
            sizeInMb: 384);

        tenant1Model1Ready.Rate(5);
        tenant1Model2Ready.Rate(5);
        tenant1Model3Ready.Rate(5);
        tenant1Model4Ready.Rate(5);
        tenant1ModelNotReady.Rate(4);
        tenant2Model1Ready.Rate(5);
        tenant2Model2Ready.Rate(5);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(tenant1TestCategory);
            await context.AddAsync(tenant1Model1Ready);
            await context.AddAsync(tenant1Model2Ready);
            await context.AddAsync(tenant1Model3Ready);
            await context.AddAsync(tenant1Model4Ready);
            await context.AddAsync(tenant1ModelNotReady);
            await context.SaveChangesAsync();
            _tenant1Model1Id = tenant1Model1Ready.Id;
            _tenant1Model2Id = tenant1Model2Ready.Id;
            _tenant1Model3Id = tenant1Model3Ready.Id;
            _tenant1Model4Id = tenant1Model4Ready.Id;
            _tenant1ModelNotReadyId = tenant1ModelNotReady.Id;

            await context.AddAsync(tenant2TestCategory);
            await context.AddAsync(tenant2Model1Ready);
            await context.AddAsync(tenant2Model2Ready);
            await context.SaveChangesAsync();
            _tenant2Model1Id = tenant2Model1Ready.Id;
            _tenant2Model2Id = tenant2Model2Ready.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Prioritize_Models()
    {
        // Prepare
        var request = new PrioritizeModels.Request
        {
            ModelIds = [_tenant1Model1Id, _tenant1Model2Id]
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request);

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);

        // Assert the database - priorities should not be changed
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Id == _tenant1Model1Id || x.Id == _tenant1Model2Id).ToList();
        models.Count.ShouldBe(2);
        models.All(x => x.Priority == 0).ShouldBeTrue(); // Should remain at default priority
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Prioritize_Models_From_Other_Tenant()
    {
        // Prepare
        var request = new PrioritizeModels.Request
        {
            ModelIds = [_tenant2Model1Id, _tenant2Model2Id] // Models from different tenant
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "All models must exist and be eligible for prioritization.");

        // Assert the database - priorities should not be changed
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.Id == _tenant2Model1Id || x.Id == _tenant2Model2Id).ToList();
        models.Count.ShouldBe(2);
        models.All(x => x.Priority == 0).ShouldBeTrue(); // Should remain at default priority
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Prioritize_Models_With_Invalid_Ids()
    {
        // Prepare
        var request = new PrioritizeModels.Request
        {
            ModelIds = [Guid.NewGuid(), Guid.NewGuid()] // Invalid model IDs
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "All models must exist and be eligible for prioritization.");

        // Assert the database - no priorities should be changed
        var models = TestingServer.CreateContext().Set<Model>()
            .Where(x => x.TenantId == "RODRIGO.BASNIAK")
            .ToList();
        models.All(x => x.Priority == 0).ShouldBeTrue(); // Should remain at default priority
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Cannot_Prioritize_Models_With_Duplicate_Ids()
    {
        // Prepare
        var request = new PrioritizeModels.Request
        {
            ModelIds = [_tenant1Model1Id, _tenant1Model1Id] // Duplicate IDs
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Model IDs must not contain duplicates.");

        // Assert the database - priorities should not be changed
        var models = TestingServer.CreateContext().Set<Model>()
            .Where(x => x.TenantId == "RODRIGO.BASNIAK")
            .ToList();
        models.All(x => x.Priority == 0).ShouldBeTrue(); // Should remain at default priority
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Cannot_Prioritize_Models_With_Empty_Array()
    {
        // Prepare
        var request = new PrioritizeModels.Request
        {
            ModelIds = [] // Empty array
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "'Model Ids' must not be empty.");

        // Assert the database - priorities should not be changed
        var models = TestingServer.CreateContext().Set<Model>()
            .Where(x => x.TenantId == "RODRIGO.BASNIAK")
            .ToList();
        models.All(x => x.Priority == 0).ShouldBeTrue(); // Should remain at default priority
    }

    [Test, NotInParallel(Order = 7)]
    public async Task User_Cannot_Prioritize_Models_With_Null_Array()
    {
        // Prepare
        var request = new PrioritizeModels.Request
        {
            ModelIds = null! // Null array
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "'Model Ids' must not be empty.");

        // Assert the database - priorities should not be changed
        var models = TestingServer.CreateContext().Set<Model>()
            .Where(x => x.TenantId == "RODRIGO.BASNIAK")
            .ToList();
        models.All(x => x.Priority == 0).ShouldBeTrue(); // Should remain at default priority
    }

    [Test, NotInParallel(Order = 8)]
    public async Task User_Cannot_Prioritize_Models_With_Empty_Guid()
    {
        // Prepare
        var request = new PrioritizeModels.Request
        {
            ModelIds = [Guid.Empty] // Empty GUID
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "All IDs in the list must be valid.");

        // Assert the database - priorities should not be changed
        var models = TestingServer.CreateContext().Set<Model>()
            .Where(x => x.TenantId == "RODRIGO.BASNIAK")
            .ToList();
        models.All(x => x.Priority == 0).ShouldBeTrue(); // Should remain at default priority
    }

    [Test, NotInParallel(Order = 9)]
    public async Task User_Cannot_Prioritize_Models_With_Mixed_Valid_And_Invalid_Ids()
    {
        // Prepare
        var request = new PrioritizeModels.Request
        {
            ModelIds = [_tenant1Model1Id, Guid.NewGuid()] // One valid, one invalid
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "All models must exist and be eligible for prioritization.");

        // Assert the database - priorities should not be changed
        var models = TestingServer.CreateContext().Set<Model>()
            .Where(x => x.TenantId == "RODRIGO.BASNIAK")
            .ToList();
        models.All(x => x.Priority == 0).ShouldBeTrue(); // Should remain at default priority
    }

    [Test, NotInParallel(Order = 10)]
    public async Task User_Cannot_Prioritize_Models_With_Mixed_Scores()
    {
        // Prepare
        var request = new PrioritizeModels.Request
        {
            ModelIds = [_tenant1Model1Id, _tenant1Model2Id, _tenant1ModelNotReadyId, _tenant1Model4Id]  
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "All models must exist and be eligible for prioritization.");

        // Assert the database - priorities should not be changed
        var models = TestingServer.CreateContext().Set<Model>().Where(x => x.TenantId == "RODRIGO.BASNIAK").ToList();
        models.All(x => x.Priority == 0).ShouldBeTrue(); // Should remain at default priority
    }

    [Test, NotInParallel(Order = 11)]
    public async Task User_Can_Prioritize_Multiple_Models_In_Order()
    {
        // Prepare
        var request = new PrioritizeModels.Request
        {
            ModelIds = [_tenant1Model1Id, _tenant1Model2Id, _tenant1Model3Id]
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - priorities should be set in reverse order
        var model1 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model1Id);
        var model2 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model2Id);
        var model3 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model3Id);

        model1.ShouldNotBeNull();
        model2.ShouldNotBeNull();
        model3.ShouldNotBeNull();

        model1.Priority.ShouldBe(3); // First in array gets highest priority (3)
        model2.Priority.ShouldBe(2); // Second in array gets priority 2
        model3.Priority.ShouldBe(1); // Third in array gets priority 1

        // Other models should have priority reset to 0
        var otherModels = TestingServer.CreateContext().Set<Model>()
            .Where(x => !request.ModelIds.Contains(x.Id) && x.TenantId == "RODRIGO.BASNIAK")
            .ToList();

        otherModels.All(x => x.Priority == 0).ShouldBeTrue();
    }

    [Test, NotInParallel(Order = 12)]
    public async Task User_Can_Reprioritize_Models_With_Different_Order()
    {
        // Prepare - first prioritize in one order
        var firstRequest = new PrioritizeModels.Request
        {
            ModelIds = [_tenant1Model1Id, _tenant1Model2Id]
        };

        await TestingServer.PostAsync("api/models/prioritize", firstRequest, "rodrigo.basniak");

        // Now reprioritize in different order
        var secondRequest = new PrioritizeModels.Request
        {
            ModelIds = [_tenant1Model2Id, _tenant1Model1Id]
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", secondRequest, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - priorities should be updated
        var model1 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model1Id);
        var model2 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model2Id);

        model1.ShouldNotBeNull();
        model2.ShouldNotBeNull();

        model2.Priority.ShouldBe(2); // First in new array gets highest priority (2)
        model1.Priority.ShouldBe(1); // Second in new array gets priority 1

        // Other models should have priority reset to 0
        var otherModels = TestingServer.CreateContext().Set<Model>()
            .Where(x => !secondRequest.ModelIds.Contains(x.Id) && x.TenantId == "RODRIGO.BASNIAK")
            .ToList();
        otherModels.All(x => x.Priority == 0).ShouldBeTrue();
    }

    [Test, NotInParallel(Order = 13)]
    public async Task User_Can_Prioritize_Models_And_Reset_Others()
    {
        // Prepare - first set some priorities manually
        using (var context = TestingServer.CreateContext())
        {
            var testModel1 = context.Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model1Id);
            var testModel2 = context.Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model2Id);
            var testModel3 = context.Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model3Id);

            testModel1.UpdatePriority(5);
            testModel2.UpdatePriority(3);
            testModel3.UpdatePriority(1);

            await context.SaveChangesAsync();
        }

        // Now prioritize only model1 and model3
        var request = new PrioritizeModels.Request
        {
            ModelIds = [_tenant1Model1Id, _tenant1Model3Id]
        };

        // Act
        var response = await TestingServer.PostAsync("api/models/prioritize", request, "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database
        var model1 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model1Id);
        var model2 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model2Id);
        var model3 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model3Id);

        model1.ShouldNotBeNull();
        model2.ShouldNotBeNull();
        model3.ShouldNotBeNull();

        model1.Priority.ShouldBe(2); // First in array gets highest priority (2)
        model3.Priority.ShouldBe(1); // Second in array gets priority 1
        model2.Priority.ShouldBe(0); // Should be reset to 0 since not in the new prioritization
    }

    [Test, NotInParallel(Order = 14)]
    public async Task Different_Tenants_Can_Prioritize_Their_Own_Models_Independently()
    {
        // Tenant 1 prioritizes their models
        var tenant1Request = new PrioritizeModels.Request
        {
            ModelIds = [_tenant1Model1Id, _tenant1Model2Id]
        };

        var tenant1Response = await TestingServer.PostAsync("api/models/prioritize", tenant1Request, "rodrigo.basniak");
        tenant1Response.ShouldBeSuccess();

        // Tenant 2 prioritizes their models
        var tenant2Request = new PrioritizeModels.Request
        {
            ModelIds = [_tenant2Model1Id, _tenant2Model2Id]
        };

        var tenant2Response = await TestingServer.PostAsync("api/models/prioritize", tenant2Request, "ricardo.smarzaro");
        tenant2Response.ShouldBeSuccess();

        // Assert both tenants' models have correct priorities
        var tenant1Model1 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model1Id);
        var tenant1Model2 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1Model2Id);
        var tenant2Model1 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant2Model1Id);
        var tenant2Model2 = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant2Model2Id);

        tenant1Model1.ShouldNotBeNull();
        tenant1Model2.ShouldNotBeNull();
        tenant2Model1.ShouldNotBeNull();
        tenant2Model2.ShouldNotBeNull();

        tenant1Model1.Priority.ShouldBe(2);
        tenant1Model2.Priority.ShouldBe(1);
        tenant2Model1.Priority.ShouldBe(2);
        tenant2Model2.Priority.ShouldBe(1);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 