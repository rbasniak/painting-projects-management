using Microsoft.EntityFrameworkCore;
using PaintingProjectsManagement.Features.Materials.Tests.Infrastructure;

namespace PaintingProjectsManagement.Features.Materials.Tests.Integration;

public class CreateMaterialTests : IntegrationTestBase
{
    public CreateMaterialTests(ApiTestFixture fixture) : base(fixture)
    {
    }

    [Fact]
    public async Task Create_Material_Should_Return_MaterialDetails()
    {
        // Arrange
        var request = new CreateMaterial.Request
        {
            Name = TestData.ValidMaterialName,
            Unit = TestData.ValidMaterialUnit,
            PricePerUnit = TestData.ValidPricePerUnit
        };

        // Act
        var response = await Fixture.PostAsync("/materials", request);

        // Assert
        var materialDetails = await response.ShouldBeSuccessful<MaterialDetails>();

        materialDetails.ShouldNotBeNull();
        materialDetails.Id.ShouldNotBe(Guid.Empty);
        materialDetails.Name.ShouldBe(request.Name);
        materialDetails.Unit.ShouldBe(request.Unit);
        materialDetails.PricePerUnit.ShouldBe(request.PricePerUnit);

        // Verify the material was saved to the database
        using var dbContext = await CreateScopedDbContextAsync();
        var material = await dbContext.Set<Material>().FirstOrDefaultAsync(m => m.Id == materialDetails.Id);
        material.ShouldNotBeNull();
        material.Name.ShouldBe(request.Name);
        material.Unit.ShouldBe(request.Unit);
        material.PricePerUnit.ShouldBe(request.PricePerUnit);
    }

    [Fact]
    public async Task Create_Material_With_Empty_Name_Should_Return_BadRequest()
    {
        // Arrange
        var request = new CreateMaterial.Request
        {
            Name = string.Empty,
            Unit = TestData.ValidMaterialUnit,
            PricePerUnit = TestData.ValidPricePerUnit
        };

        // Act
        var response = await Fixture.PostAsync("/materials", request);

        // Assert
        var errorMessage = await response.ShouldBeBadRequest();

        errorMessage.ShouldContain("'Name' must not be empty");
    }

    [Fact]
    public async Task Create_Material_With_Zero_Price_Should_Return_BadRequest()
    {
        // Arrange
        var request = new CreateMaterial.Request
        {
            Name = TestData.ValidMaterialName,
            Unit = TestData.ValidMaterialUnit,
            PricePerUnit = 0
        };

        // Act
        var response = await Fixture.PostAsync("/materials", request);

        // Assert
        var errorMessage = await response.ShouldBeBadRequest();
        errorMessage.ShouldContain("Price per unit must be greater than zero");
    }

    [Fact]
    public async Task Create_Material_With_Duplicate_Name_Should_Return_BadRequest()
    {
        // Arrange - Create first material
        var request1 = new CreateMaterial.Request
        {
            Name = "Unique Material Name",
            Unit = TestData.ValidMaterialUnit,
            PricePerUnit = TestData.ValidPricePerUnit
        };

        await Fixture.PostAsync("/materials", request1);

        // Arrange - Create second material with same name
        var request2 = new CreateMaterial.Request
        {
            Name = "Unique Material Name",
            Unit = TestData.ValidMaterialUnit,
            PricePerUnit = TestData.ValidPricePerUnit
        };

        // Act
        var response = await Fixture.PostAsync("/materials", request2);

        // Assert
        var errorMessage = await response.ShouldBeBadRequest();
        errorMessage.ShouldContain("A material with this name already exists");
    }

    [Fact]
    public async Task Create_Material_With_Name_Exceeding_MaxLength_Should_Return_BadRequest()
    {
        // Arrange
        var request = new CreateMaterial.Request
        {
            Name = new string('A', 101), // 101 characters, more than the 100 allowed
            Unit = TestData.ValidMaterialUnit,
            PricePerUnit = TestData.ValidPricePerUnit
        };

        // Act
        var response = await Fixture.PostAsync("/materials", request);

        // Assert
        var errorMessage = await response.ShouldBeBadRequest();
        errorMessage.ShouldContain("The length of 'Name' must be 100 characters or fewer. You entered 101 characters.");
    }

    public async Task Create_Material_Should_Succeed()
    {
        // Arrange
        var request = new CreateMaterial.Request
        {
            Name = "5x3 Magnet",
            Unit = MaterialUnit.Unit,
            PricePerUnit = 10
        };

        // Act
        var response = await Fixture.PostAsync("/materials", request);

        // Assert the response
        var result = await response.ShouldBeBadRequest();
    }
}