using PaintingProjectsManagement.Features.Models;

namespace PaintingProjectsManagement.Features.Models.Tests;

[HumanFriendlyDisplayName]
public class Model_Pictures_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private readonly string _base64Image1 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAQSURBVBhXY/iPBkgW+P8fAHg8P8Hpkr/2AAAAAElFTkSuQmCC";
    private readonly string _base64Image2 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAQAAAAECAYAAACp8Z5+AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAATSURBVBhXY2BY9OI/CiZV4MV/AEmXKJHeHyJtAAAAAElFTkSuQmCC";

    private static Guid _modelId;

    [Test, NotInParallel(Order = -1)]
    public async Task Seed()
    {
        var model = await CreateTestModel(Guid.NewGuid().ToString());
        _modelId = model.Id;

        // Login with the users that will be used in the tests, so they will be cached in the TestingServer for easy access
        await TestingServer.CacheCredentialsAsync("rodrigo.basniak", "trustno1", "rodrigo.basniak");
    }

    [Test, NotInParallel(Order = 1)]
    public async Task Upload_Model_Picture_Should_Add_To_Pictures_Array()
    {
        // Act
        var response = await TestingServer.PostAsync(
            "api/models/picture",
            new UploadModelPicture.Request
            {
                ModelId = _modelId,
                Base64Image = _base64Image1,
            }, "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess();

        // Verify the picture was added to the Pictures array
        var updatedModel = await TestingServer.CreateContext().Set<Model>().FirstAsync(x => x.Id == _modelId);
        updatedModel.Pictures.ShouldNotBeEmpty();
        updatedModel.Pictures.Length.ShouldBe(1);
        updatedModel.CoverPicture.ShouldBeNull(); // Cover picture should remain unchanged
    }

    [Test, NotInParallel(Order = 2)]
    public async Task Upload_Multiple_Model_Pictures_Should_Add_To_Pictures_Array()
    {
        // Act
        var response1 = await TestingServer.PostAsync(
            "api/models/picture",
            new UploadModelPicture.Request
            {
                ModelId = _modelId,
                Base64Image = _base64Image1
            }, "rodrigo.basniak");

        var response2 = await TestingServer.PostAsync(
            "api/models/picture",
            new UploadModelPicture.Request
            {
                ModelId = _modelId,
                Base64Image = _base64Image2
            }, "rodrigo.basniak");

        // Assert
        response1.ShouldBeSuccess();
        response2.ShouldBeSuccess();

        // Verify both pictures were added to the Pictures array
        var updatedModel = await TestingServer.CreateContext().Set<Model>().FirstAsync(x => x.Id == _modelId);
        updatedModel.Pictures.Length.ShouldBe(3); // 1 from previous test + 2 new ones
        updatedModel.CoverPicture.ShouldBeNull(); // Cover picture should remain unchanged
    }

    [Test, NotInParallel(Order = 3)]
    public async Task Promote_Picture_To_Cover_Should_Set_CoverPicture()
    {
        // Arrange
        var modelWithPicture = await TestingServer.CreateContext().Set<Model>().FirstAsync(x => x.Id == _modelId);
        var pictureUrl = modelWithPicture.Pictures.First();

        // Act
        var response = await TestingServer.PostAsync(
            $"api/models/picture/promote",
            new PromotePictureToCover.Request
            {
                ModelId = _modelId,
                PictureUrl = pictureUrl
            }, "rodrigo.basniak");

        // Assert
        response.ShouldBeSuccess();

        // Verify the picture was set as cover picture
        var updatedModel = await TestingServer.CreateContext().Set<Model>().FirstAsync(x => x.Id == _modelId);
        updatedModel.CoverPicture.ShouldBe(pictureUrl);
        updatedModel.Pictures.ShouldContain(pictureUrl); // Picture should still be in the array
    }

    [Test, NotInParallel(Order = 4)]
    public async Task Promote_Non_Existent_Picture_Should_Fail()
    {
        // Arrange
        var nonExistentPictureUrl = "https://example.com/non-existent-picture.jpg";

        // Act
        var response = await TestingServer.PostAsync(
            $"api/models/picture/promote",
            new PromotePictureToCover.Request
            {
                ModelId = _modelId,
                PictureUrl = nonExistentPictureUrl
            }, "rodrigo.basniak");

        // Assert
        response.ShouldHaveErrors(HttpStatusCode.BadRequest, "The specified picture URL must exist in the model's pictures collection.");
    }

    [Test, NotInParallel(Order = 5)]
    public async Task Upload_Invalid_Base64_Image_Should_Fail()
    {
        // Arrange
        var invalidBase64 = "invalid-base64-data";

        // Act
        var response = await TestingServer.PostAsync(
            "api/models/picture",
            new UploadModelPicture.Request
            {
                ModelId = _modelId,
                Base64Image = invalidBase64
            }, "rodrigo.basniak");

        // Assert
        response.ShouldHaveErrors(System.Net.HttpStatusCode.BadRequest, "Invalid image format.");
    }

    [Test, NotInParallel(Order = 6)]
    public async Task Upload_Empty_Base64_Image_Should_Fail()
    {
        // Arrange
        // Act
        var response = await TestingServer.PostAsync(
            "api/models/picture",
            new UploadModelPicture.Request
            {
                ModelId = _modelId,
                Base64Image = string.Empty
            }, "rodrigo.basniak");

        // Assert
        response.ShouldHaveErrors(System.Net.HttpStatusCode.BadRequest, "'Base64 Image' must not be empty.");
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Delete_Model_Picture_Should_Remove_From_Pictures_Array()
    {
        var model = await TestingServer.CreateContext().Set<Model>().FirstAsync(x => x.Id == _modelId);
        var pictureUrl = model.Pictures.First();

        var response = await TestingServer.PostAsync($"api/models/picture/delete", 
            new DeleteModelPicture.Request { ModelId = _modelId, PictureUrl = pictureUrl }, 
            "rodrigo.basniak");

        response.ShouldBeSuccess();

        var updated = await TestingServer.CreateContext().Set<Model>().FirstAsync(x => x.Id == _modelId);
        updated.Pictures.ShouldNotContain(pictureUrl);
    }

    [Test, NotInParallel(Order = 99)]
    public async Task Cleanup()
    {
        await TestingServer.DisposeAsync();
    }

    private async Task<Model> CreateTestModel(string name)
    {
        var category = new ModelCategory("rodrigo.basniak", "Test Category");
        var model = new Model(
            "rodrigo.basniak",
            name,
            category,
            ["Character1", "Character2"],
            "Test Franchise",
            ModelType.Figure,
            "Test Artist",
            ["Tag1", "Tag2"],
            BaseSize.Small,
            FigureSize.Normal,
            1,
            100
        );

        var context = TestingServer.CreateContext();
        context.Set<ModelCategory>().Add(category);
        context.Set<Model>().Add(model);
        await context.SaveChangesAsync();

        return model;
    }
}
