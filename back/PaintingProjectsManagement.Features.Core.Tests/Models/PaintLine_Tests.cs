namespace PaintingProjectsManagement.Features.Inventory.Core.Tests;

public class PaintLine_Tests
{
    [Test]
    public void Constructor_Creates_PaintLine_With_Correct_Name_And_Brand()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var name = "Base";

        // Act
        var line = new PaintLine(brand, name);

        // Assert
        line.Name.ShouldBe(name);
        line.Brand.ShouldBe(brand);
    }

    [Test]
    public void Constructor_Creates_PaintLine_With_Empty_Name()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var name = "";

        // Act
        var line = new PaintLine(brand, name);

        // Assert
        line.Name.ShouldBe(name);
        line.Brand.ShouldBe(brand);
    }

    [Test]
    public void Constructor_Sets_BrandId_Correctly()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var name = "Base";

        // Act
        var line = new PaintLine(brand, name);

        // Assert
        line.BrandId.ShouldBe(brand.Id);
    }

    [Test]
    public void UpdateDetails_Updates_Name_Correctly()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");
        var newName = "Layer";

        // Act
        line.UpdateDetails(newName);

        // Assert
        line.Name.ShouldBe(newName);
    }

    [Test]
    public void UpdateDetails_Does_Not_Change_Brand()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");
        var originalBrand = line.Brand;
        var originalBrandId = line.BrandId;

        // Act
        line.UpdateDetails("Layer");

        // Assert
        line.Brand.ShouldBe(originalBrand);
        line.BrandId.ShouldBe(originalBrandId);
    }

    [Test]
    public void UpdateDetails_Can_Update_To_Empty_Name()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");
        var newName = "";

        // Act
        line.UpdateDetails(newName);

        // Assert
        line.Name.ShouldBe(newName);
    }

    [Test]
    public void UpdateDetails_Can_Be_Called_Multiple_Times()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");

        // Act
        line.UpdateDetails("Layer");
        line.UpdateDetails("Shade");
        line.UpdateDetails("Contrast");

        // Assert
        line.Name.ShouldBe("Contrast");
    }
}
