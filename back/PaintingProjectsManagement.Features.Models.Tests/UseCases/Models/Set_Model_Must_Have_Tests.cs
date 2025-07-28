using PaintingProjectsManagement.Features.Models.Tests;
using System.Net;
using Shouldly;

namespace PaintingProjectsManagement.Features.Models.Tests;

public class Set_Model_Must_Have_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _tenant1ModelId;
    private static Guid _tenant2ModelId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        // Create test model categories for the tests
        var tenant1Category = new ModelCategory("rodrigo.basniak", "Tenant 1 Category");
        var tenant2Category = new ModelCategory("ricardo.smarzaro", "Tenant 2 Category");

        // Create models for testing
        var tenant1Model = new Model(
            tenant: "rodrigo.basniak",
            name: "Tenant 1 Model",
            category: tenant1Category,
            characters: ["Character1"],
            franchise: "Franchise1",
            type: ModelType.Figure,
            artist: "Artist1",
            tags: ["tag1"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 512);

        var tenant2Model = new Model(
            tenant: "ricardo.smarzaro",
            name: "Tenant 2 Model",
            category: tenant2Category,
            characters: ["Character2"],
            franchise: "Franchise2",
            type: ModelType.Miniature,
            artist: "Artist2",
            tags: ["tag2"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 256);

        using (var context = TestingServer.CreateContext())
        {
            // Add categories first
            await context.AddAsync(tenant1Category);
            await context.AddAsync(tenant2Category);
            await context.SaveChangesAsync();

            // Add models
            await context.AddAsync(tenant1Model);
            await context.AddAsync(tenant2Model);
            await context.SaveChangesAsync();

            // Store IDs for assertions
            _tenant1ModelId = tenant1Model.Id;
            _tenant2ModelId = tenant2Model.Id;
        }

        // Login with the users that will be used in the tests
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
        await TestingServer.CacheCredentialsAsync("ricardo.smarzaro", "zemiko987", "ricardo.smarzaro");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Non_Authenticated_User_Cannot_Set_Must_Have()
    {
        // Prepare
        var request = new SetModelMustHave.Request
        {
            MustHave = true
        };

        // Act
        var response = await TestingServer.PutAsync($"api/models/{_tenant1ModelId}/must-have", request);

        // Assert
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task User_Cannot_Set_Must_Have_For_Model_From_Other_Tenant()
    {
        // Prepare
        var request = new SetModelMustHave.Request
        {
            MustHave = true
        };

        // Act
        var response = await TestingServer.PutAsync($"api/models/{_tenant2ModelId}/must-have", request, "rodrigo.basniak");

        // Assert
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");

        // Verify the model was not changed
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant2ModelId);
        model.ShouldNotBeNull();
        model.MustHave.ShouldBeFalse(); // Should remain false
    }

    [Test, NotInParallel(Order = 4)]
    public async Task User_Cannot_Set_Must_Have_For_Non_Existent_Model()
    {
        // Prepare
        var request = new SetModelMustHave.Request
        {
            MustHave = true
        };

        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await TestingServer.PutAsync($"api/models/{nonExistentId}/must-have", request, "rodrigo.basniak");

        // Assert
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "Id references a non-existent record.");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task User_Can_Set_Must_Have_To_True()
    {
        // Prepare
        var request = new SetModelMustHave.Request
        {
            MustHave = true
        };

        // Act
        var response = await TestingServer.PutAsync($"api/models/{_tenant1ModelId}/must-have", request, "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess();

        // Verify the model was updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.MustHave.ShouldBeTrue();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task User_Can_Set_Must_Have_To_False()
    {
        // Prepare - first set to true
        var setTrueRequest = new SetModelMustHave.Request
        {
            MustHave = true
        };
        await TestingServer.PutAsync($"api/models/{_tenant1ModelId}/must-have", setTrueRequest, "rodrigo.basniak");

        // Now set to false
        var setFalseRequest = new SetModelMustHave.Request
        {
            MustHave = false
        };

        // Act
        var response = await TestingServer.PutAsync($"api/models/{_tenant1ModelId}/must-have", setFalseRequest, "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess();

        // Verify the model was updated
        var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        model.ShouldNotBeNull();
        model.MustHave.ShouldBeFalse();
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Different_Tenants_Can_Set_Must_Have_For_Their_Own_Models_Independently()
    {
        // Tenant 1 sets their model to true
        var tenant1Request = new SetModelMustHave.Request
        {
            MustHave = true
        };
        var tenant1Response = await TestingServer.PutAsync($"api/models/{_tenant1ModelId}/must-have", tenant1Request, "rodrigo.basniak");
        tenant1Response.ShouldBeSuccess();

        // Tenant 2 sets their model to false
        var tenant2Request = new SetModelMustHave.Request
        {
            MustHave = false
        };
        var tenant2Response = await TestingServer.PutAsync($"api/models/{_tenant2ModelId}/must-have", tenant2Request, "ricardo.smarzaro");
        tenant2Response.ShouldBeSuccess();

        // Verify both models have correct values
        var tenant1Model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
        var tenant2Model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant2ModelId);

        tenant1Model.ShouldNotBeNull();
        tenant2Model.ShouldNotBeNull();

        tenant1Model.MustHave.ShouldBeTrue();
        tenant2Model.MustHave.ShouldBeFalse();
    }

    [Test, NotInParallel(Order = 8)]
    public async Task User_Can_Toggle_Must_Have_Multiple_Times()
    {
        // Toggle multiple times
        for (int i = 0; i < 3; i++)
        {
            var request = new SetModelMustHave.Request
            {
                MustHave = i % 2 == 0 // Alternate between true and false
            };

            var response = await TestingServer.PutAsync($"api/models/{_tenant1ModelId}/must-have", request, "rodrigo.basniak");
            response.ShouldBeSuccess();

            // Verify the model was updated correctly
            var model = TestingServer.CreateContext().Set<Model>().FirstOrDefault(x => x.Id == _tenant1ModelId);
            model.ShouldNotBeNull();
            model.MustHave.ShouldBe(i % 2 == 0);
        }
    }

    [Test, NotInParallel(Order = 99)]
    public async Task CleanUp()
    {
        await TestingServer.CreateContext().Database.EnsureDeletedAsync();
    }
} 