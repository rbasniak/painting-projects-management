namespace PaintingProjectsManagement.Features.Models.Tests;

public class Model_Pictures_Tests
{
    [ClassDataSource<TestingServer>(Shared = SharedType.PerClass)]
    public required TestingServer TestingServer { get; set; } = default!;

    private static Guid _modelId;
    private static string _tenantId = "test-tenant";

    [Test, NotInParallel(Order = 1)]
    public async Task Upload_Model_Picture_Should_Add_To_Pictures_Array()
    {
        // Arrange
        _modelId = Guid.NewGuid();
        var model = await CreateTestModel();
        var base64Image = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/2wBDAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAv/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/8A";

        // Act
        var response = await TestingServer.PostAsync(
            "api/models/picture",
            new UploadModelPicture.Request
            {
                ModelId = _modelId,
                Base64Image = base64Image
            });

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
        // Arrange
        var base64Image1 = "data:image/jpeg;base64,/9j/4AAQSkZJRgABAQEAYABgAAD/2wBDAAEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/2wBDAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQEBAQH/wAARCAABAAEDASIAAhEBAxEB/8QAFQABAQAAAAAAAAAAAAAAAAAAAAv/xAAUEAEAAAAAAAAAAAAAAAAAAAAA/8QAFQEBAQAAAAAAAAAAAAAAAAAAAAX/xAAUEQEAAAAAAAAAAAAAAAAAAAAA/9oADAMBAAIRAxEAPwA/8A";
        var base64Image2 = "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==";

        // Act
        var response1 = await TestingServer.PostAsync(
            "api/models/picture",
            new UploadModelPicture.Request
            {
                ModelId = _modelId,
                Base64Image = base64Image1
            });

        var response2 = await TestingServer.PostAsync(
            "api/models/picture",
            new UploadModelPicture.Request
            {
                ModelId = _modelId,
                Base64Image = base64Image2
            });

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
            $"api/models/{_modelId}/promote-picture",
            new PromotePictureToCover.Request
            {
                ModelId = _modelId,
                PictureUrl = pictureUrl
            });

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
            $"api/models/{_modelId}/promote-picture",
            new PromotePictureToCover.Request
            {
                ModelId = _modelId,
                PictureUrl = nonExistentPictureUrl
            });

        // Assert
        response.ShouldHaveErrors(System.Net.HttpStatusCode.BadRequest, "The specified picture URL must exist in the model's pictures collection.");
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
            });

        // Assert
        response.ShouldHaveErrors(System.Net.HttpStatusCode.BadRequest, "Invalid base64 image format. Must be a valid base64 encoded image with proper header.");
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
            });

        // Assert
        response.ShouldHaveErrors(System.Net.HttpStatusCode.BadRequest, "Base64 image content is required.");
    }

    [Test, NotInParallel(Order = 7)]
    public async Task Delete_Model_Picture_Should_Remove_From_Pictures_Array()
    {
        var model = await TestingServer.CreateContext().Set<Model>().FirstAsync(x => x.Id == _modelId);
        var pictureUrl = model.Pictures.First();

        var response = await TestingServer.DeleteAsync($"api/models/{_modelId}/picture?pictureUrl={Uri.EscapeDataString(pictureUrl)}");

        response.ShouldBeSuccess();

        var updated = await TestingServer.CreateContext().Set<Model>().FirstAsync(x => x.Id == _modelId);
        updated.Pictures.ShouldNotContain(pictureUrl);
    }

    private async Task<Model> CreateTestModel()
    {
        var category = new ModelCategory(_tenantId, "Test Category");
        var model = new Model(
            _tenantId,
            "Test Model",
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

        // Override the ID for testing
        var modelReflection = typeof(Model).GetField("_id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        modelReflection?.SetValue(model, _modelId);

        var context = TestingServer.CreateContext();
        context.Set<ModelCategory>().Add(category);
        context.Set<Model>().Add(model);
        await context.SaveChangesAsync();

        return model;
    }
}
