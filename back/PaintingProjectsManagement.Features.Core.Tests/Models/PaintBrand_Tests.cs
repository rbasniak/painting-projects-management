namespace PaintingProjectsManagement.Features.Inventory.Core.Tests;

public class PaintBrand_Tests
{
    [Test]
    public void Constructor_Creates_PaintBrand_With_Correct_Name()
    {
        // Arrange
        var name = "Citadel";

        // Act
        var brand = new PaintBrand(name);

        // Assert
        brand.Name.ShouldBe(name);
    }

    [Test]
    public void Constructor_Creates_PaintBrand_With_Empty_Name()
    {
        // Arrange
        var name = "";

        // Act
        var brand = new PaintBrand(name);

        // Assert
        brand.Name.ShouldBe(name);
    }

    [Test]
    public void UpdateDetails_Updates_Name_Correctly()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var newName = "Vallejo";

        // Act
        brand.UpdateDetails(newName);

        // Assert
        brand.Name.ShouldBe(newName);
    }

    [Test]
    public void UpdateDetails_Can_Update_To_Empty_Name()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var newName = "";

        // Act
        brand.UpdateDetails(newName);

        // Assert
        brand.Name.ShouldBe(newName);
    }

    [Test]
    public void UpdateDetails_Can_Be_Called_Multiple_Times()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");

        // Act
        brand.UpdateDetails("Vallejo");
        brand.UpdateDetails("Army Painter");
        brand.UpdateDetails("Scale75");

        // Assert
        brand.Name.ShouldBe("Scale75");
    }
}
