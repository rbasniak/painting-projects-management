namespace PaintingProjectsManagement.Features.Models.Tests;

public class Delete_Model_Tests
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
    public async Task Non_Authenticated_User_Cannot_Delete_Model()
    {
        // Prepare - Use a non-existent model ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"api/models/{nonExistentId}");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Delete_Model_That_Does_Not_Exist()
    {
        // Prepare - Use a non-existent model ID
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.DeleteAsync($"api/models/{nonExistentId}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Delete_Model_That_Belongs_To_Another_User()
    {
        // Prepare - Load the model created by another user
        var anotherUserModel = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Name == "Model 2");
        anotherUserModel.ShouldNotBeNull("Model should exist from seed");
        anotherUserModel.TenantId.ShouldBe("RICARDO.SMARZARO", "Model should belong to another user");

        // Act - Try to delete as rodrigo.basniak (different user)
        var response = await TestingServer.DeleteAsync($"api/models/{anotherUserModel.Id}", "rodrigo.basniak");

        // Assert the response
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Assert the database - model should still exist
        var stillExistingEntity = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == anotherUserModel.Id);
        stillExistingEntity.ShouldNotBeNull("Model should still exist in database");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Can_Delete_Model()
    {
        // Prepare - Load the model created in the seed
        var existingModel = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Name == "Model 1");
        existingModel.ShouldNotBeNull("Model should exist from seed");
        existingModel.TenantId.ShouldBe("RODRIGO.BASNIAK", "Model should belong to the current user");

        // Act
        var response = await TestingServer.DeleteAsync($"api/models/{existingModel.Id}", "rodrigo.basniak");

        // Assert the response
        response.ShouldBeSuccess();

        // Assert the database - model should be deleted
        var deletedEntity = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == existingModel.Id);
        deletedEntity.ShouldBeNull("Model should be deleted from database");
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
} 