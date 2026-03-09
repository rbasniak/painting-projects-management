using System.Text.Json;

namespace PaintingProjectsManagement.Features.Models.Tests;

[HumanFriendlyDisplayName]
public class Integrations_Models_Api_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _tenant1CategoryId;
    private static Guid _existingModelId;
    private static Guid _guidDeleteModelId;
    private static string _tenant1ApiKey = string.Empty;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        using var context = TestingServer.CreateContext();

        var user = await context.Set<User>()
            .FirstAsync(x => x.Username == "rodrigo.basniak");

        _tenant1ApiKey = user.Id.ToString();

        var category = new ModelCategory("rodrigo.basniak", "Integrations Category");
        var existingModel = new Model(
            tenant: "rodrigo.basniak",
            name: "Existing Integration Model",
            category: category,
            characters: ["Champion"],
            franchise: "Warhammer",
            type: ModelType.Figure,
            artist: "Team",
            tags: ["existing"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 20,
            identity: "external-existing");

        var guidDeleteModel = new Model(
            tenant: "rodrigo.basniak",
            name: "Delete By Guid",
            category: category,
            characters: ["Guardian"],
            franchise: "Warhammer",
            type: ModelType.Figure,
            artist: "Team",
            tags: ["delete-guid"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 15,
            identity: "external-guid-delete");

        await context.AddAsync(category);
        await context.AddAsync(existingModel);
        await context.AddAsync(guidDeleteModel);
        await context.SaveChangesAsync();

        _tenant1CategoryId = category.Id;
        _existingModelId = existingModel.Id;
        _guidDeleteModelId = guidDeleteModel.Id;
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Integrations_Api_Requires_Api_Key()
    {
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/integrations/models");
        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Upsert_Creates_Model_When_Identity_Does_Not_Exist()
    {
        var request = new
        {
            Id = "external-create-1",
            CategoryId = _tenant1CategoryId,
            Artist = "External Artist",
            Tags = new[] { "tag-a", "tag-b" },
            Characters = new[] { "Knight" },
            Name = "External Created Model",
            BaseSize = BaseSize.Big,
            FigureSize = FigureSize.Big,
            NumberOfFigures = 3,
            Franchise = "Warhammer 40k",
            Type = ModelType.Miniature,
            SizeInMb = 88
        };

        var response = await TestingServer.PutAsync<ModelDetails>("api/integrations/models", request, new ApiKey(_tenant1ApiKey));
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Name.ShouldBe("External Created Model");
        result.Identity.ShouldBe("external-create-1");

        using var context = TestingServer.CreateContext();
        var created = await context.Set<Model>()
            .SingleOrDefaultAsync(x => x.Identity == "external-create-1");

        created.ShouldNotBeNull();
        created.Name.ShouldBe("External Created Model");
        created.NumberOfFigures.ShouldBe(3);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Upsert_Updates_Model_When_Identity_Already_Exists()
    {
        var request = new
        {
            Id = "external-existing",
            CategoryId = _tenant1CategoryId,
            Artist = "Updated External Artist",
            Tags = new[] { "updated" },
            Characters = new[] { "Hero", "Companion" },
            Name = "Existing Integration Model Updated",
            BaseSize = BaseSize.Small,
            FigureSize = FigureSize.Normal,
            NumberOfFigures = 2,
            Franchise = "The Old World",
            Type = ModelType.Bust,
            SizeInMb = 45
        };

        var response = await TestingServer.PutAsync<ModelDetails>("api/integrations/models", request, new ApiKey(_tenant1ApiKey));
        response.ShouldBeSuccess(out var result);
        result.ShouldNotBeNull();
        result.Id.ShouldBe(_existingModelId);
        result.Name.ShouldBe("Existing Integration Model Updated");
        result.Identity.ShouldBe("external-existing");

        using var context = TestingServer.CreateContext();
        var updated = await context.Set<Model>().SingleAsync(x => x.Id == _existingModelId);
        updated.Artist.ShouldBe("Updated External Artist");
        updated.NumberOfFigures.ShouldBe(2);
        updated.Identity.ShouldBe("external-existing");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Delete_Removes_Model_When_Id_Is_Guid_String()
    {
        var request = new { Id = _guidDeleteModelId.ToString() };

        var response = await TestingServer.PostAsync("api/integrations/models/delete", request, new ApiKey(_tenant1ApiKey));
        response.ShouldBeSuccess();

        using var context = TestingServer.CreateContext();
        var deleted = await context.Set<Model>().FirstOrDefaultAsync(x => x.Id == _guidDeleteModelId);
        deleted.ShouldBeNull();
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Delete_Removes_Model_When_Id_Is_External_Identity()
    {
        using var setupContext = TestingServer.CreateContext();

        var category = await setupContext.Set<ModelCategory>()
            .FirstAsync(x => x.Id == _tenant1CategoryId);

        var model = new Model(
            tenant: "rodrigo.basniak",
            name: "Delete By External",
            category: category,
            characters: ["Scout"],
            franchise: "AOS",
            type: ModelType.Figure,
            artist: "Team",
            tags: ["delete-external"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 7,
            identity: "external-delete-me");

        await setupContext.AddAsync(model);
        await setupContext.SaveChangesAsync();

        var response = await TestingServer.PostAsync(
            "api/integrations/models/delete",
            new { Id = "external-delete-me" },
            new ApiKey(_tenant1ApiKey));
        response.ShouldBeSuccess();

        using var assertContext = TestingServer.CreateContext();
        var deleted = await assertContext.Set<Model>()
            .FirstOrDefaultAsync(x => x.TenantId == "rodrigo.basniak" && x.Identity == "external-delete-me");

        deleted.ShouldBeNull();
    }

    [Test, NotInParallel(Order = 7)]
    public async Task List_Returns_All_Models_For_Api_Key_User()
    {
        var response = await TestingServer.GetAsync("api/integrations/models", new ApiKey(_tenant1ApiKey));
        response.ShouldBeSuccess();

        var models = JsonSerializer.Deserialize<List<ModelDetails>>(
            response.Body,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        models.ShouldNotBeNull();
        models.Count.ShouldBeGreaterThan(0);

        var existing = models.FirstOrDefault(x => x.Id == _existingModelId);
        existing.ShouldNotBeNull();
        existing.Identity.ShouldBe("external-existing");
        existing.Category.Id.ShouldBe(_tenant1CategoryId);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
