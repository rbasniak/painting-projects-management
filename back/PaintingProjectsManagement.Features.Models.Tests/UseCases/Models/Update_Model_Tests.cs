using PaintingProjectsManagement.Features.Models.Tests;

namespace PaintingProjectsManagement.Features.Models.Tests;

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
     

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 