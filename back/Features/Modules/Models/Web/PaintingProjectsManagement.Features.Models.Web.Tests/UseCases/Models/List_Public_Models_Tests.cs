namespace PaintingProjectsManagement.Features.Models.Tests;

[HumanFriendlyDisplayName]
public class List_Public_Models_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _ownerModel1Id;
    private static Guid _ownerModel2Id;
    private static Guid _otherOwnerModelId;

    [Test, NotInParallel(Order = 1)]
    public async Task Seed()
    {
        var ownerCategory = new ModelCategory("rodrigo.basniak", "Owner Category");
        var ownerSecondCategory = new ModelCategory("rodrigo.basniak", "Owner Category 2");
        var otherOwnerCategory = new ModelCategory("ricardo.smarzaro", "Other Owner Category");

        var ownerModel1 = new Model(
            tenant: "rodrigo.basniak",
            name: "Owner Model 1",
            category: ownerCategory,
            characters: ["A", "B"],
            franchise: "Franchise A",
            type: ModelType.Miniature,
            artist: "Artist A",
            tags: ["tag-a"],
            baseSize: BaseSize.Small,
            figureSize: FigureSize.Normal,
            numberOfFigures: 1,
            sizeInMb: 50);

        var ownerModel2 = new Model(
            tenant: "rodrigo.basniak",
            name: "Owner Model 2",
            category: ownerSecondCategory,
            characters: ["C"],
            franchise: "Franchise B",
            type: ModelType.Figure,
            artist: "Artist B",
            tags: ["tag-b"],
            baseSize: BaseSize.Medium,
            figureSize: FigureSize.Normal,
            numberOfFigures: 2,
            sizeInMb: 75);

        var otherOwnerModel = new Model(
            tenant: "ricardo.smarzaro",
            name: "Other Owner Model",
            category: otherOwnerCategory,
            characters: ["Z"],
            franchise: "Franchise Z",
            type: ModelType.Figure,
            artist: "Artist Z",
            tags: ["tag-z"],
            baseSize: BaseSize.Big,
            figureSize: FigureSize.Big,
            numberOfFigures: 3,
            sizeInMb: 100);

        using (var context = TestingServer.CreateContext())
        {
            await context.AddAsync(ownerCategory);
            await context.AddAsync(ownerSecondCategory);
            await context.AddAsync(otherOwnerCategory);
            await context.AddAsync(ownerModel1);
            await context.AddAsync(ownerModel2);
            await context.AddAsync(otherOwnerModel);
            await context.SaveChangesAsync();
        }

        _ownerModel1Id = ownerModel1.Id;
        _ownerModel2Id = ownerModel2.Id;
        _otherOwnerModelId = otherOwnerModel.Id;

        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Anonymous_User_Can_List_Public_Models_For_An_Owner()
    {
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models/public/rodrigo.basniak");

        response.ShouldBeSuccess();
        response.Data.Count.ShouldBe(2);
        response.Data.ShouldContain(x => x.Id == _ownerModel1Id);
        response.Data.ShouldContain(x => x.Id == _ownerModel2Id);
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Public_List_Does_Not_Return_Models_From_Other_Owners()
    {
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models/public/rodrigo.basniak");

        response.ShouldBeSuccess();
        response.Data.ShouldNotContain(x => x.Id == _otherOwnerModelId);
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Public_List_Accepts_Uppercase_Owner_Key()
    {
        var response = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>("api/models/public/RODRIGO.BASNIAK");

        response.ShouldBeSuccess();
        response.Data.Count.ShouldBe(2);
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Anonymous_User_Cannot_Read_Their_Owner_Key()
    {
        var response = await TestingServer.GetAsync<PublicModelsOwnerKey>("api/models/public/owner-key");

        response.ShouldHaveErrors(HttpStatusCode.Unauthorized);
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Authenticated_User_Can_Read_Their_Public_Owner_Key()
    {
        var response = await TestingServer.GetAsync<PublicModelsOwnerKey>("api/models/public/owner-key", "rodrigo.basniak");

        response.ShouldBeSuccess();
        response.Data.OwnerKey.ShouldNotBeNullOrWhiteSpace();
        response.Data.OwnerKey.ShouldNotContain(".");

        var listResponse = await TestingServer.GetAsync<IReadOnlyCollection<ModelDetails>>($"api/models/public/{response.Data.OwnerKey}");
        listResponse.ShouldBeSuccess();
        listResponse.Data.Count.ShouldBe(2);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }
}
