namespace PaintingProjectsManagement.Features.Inventory.Core.Tests;

public class PaintColor_Tests
{
    [Test]
    public void Constructor_Creates_PaintColor_With_All_Required_Properties()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");
        var name = "Abaddon Black";
        var hexColor = "#000000";
        var bottleSize = 12.0;
        var type = PaintType.Opaque;

        // Act
        var color = new PaintColor(line, name, hexColor, bottleSize, type);

        // Assert
        color.Name.ShouldBe(name);
        color.HexColor.ShouldBe(hexColor);
        color.BottleSize.ShouldBe(bottleSize);
        color.Type.ShouldBe(type);
        color.Line.ShouldBe(line);
        color.LineId.ShouldBe(line.Id);
        color.ManufacturerCode.ShouldBeNull();
    }

    [Test]
    public void Constructor_Creates_PaintColor_With_ManufacturerCode()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");
        var name = "Abaddon Black";
        var hexColor = "#000000";
        var bottleSize = 12.0;
        var type = PaintType.Opaque;
        var manufacturerCode = "21-25";

        // Act
        var color = new PaintColor(line, name, hexColor, bottleSize, type, manufacturerCode);

        // Assert
        color.ManufacturerCode.ShouldBe(manufacturerCode);
    }

    [Test]
    [Arguments(PaintType.Opaque)]
    [Arguments(PaintType.Metallic)]
    [Arguments(PaintType.Wash)]
    [Arguments(PaintType.Ink)]
    [Arguments(PaintType.Contrast)]
    [Arguments(PaintType.Transparent)]
    public void Constructor_Works_With_All_Paint_Types(PaintType paintType)
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");

        // Act
        var color = new PaintColor(line, "Test Paint", "#FF0000", 12.0, paintType);

        // Assert
        color.Type.ShouldBe(paintType);
    }

    [Test]
    public void Constructor_Sets_LineId_Correctly()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");

        // Act
        var color = new PaintColor(line, "Test Paint", "#FF0000", 12.0, PaintType.Opaque);

        // Assert
        color.LineId.ShouldBe(line.Id);
    }

    [Test]
    public void Constructor_Accepts_Different_Bottle_Sizes()
    {
        // Arrange
        var brand = new PaintBrand("Vallejo");
        var line = new PaintLine(brand, "Model Color");
        var bottleSize = 17.0;

        // Act
        var color = new PaintColor(line, "Test Paint", "#FF0000", bottleSize, PaintType.Opaque);

        // Assert
        color.BottleSize.ShouldBe(bottleSize);
    }

    [Test]
    public void UpdateDetails_Updates_All_Properties_Correctly()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");
        var color = new PaintColor(line, "Old Name", "#000000", 12.0, PaintType.Opaque);

        var newName = "New Name";
        var newHexColor = "#FFFFFF";
        var newBottleSize = 18.0;
        var newType = PaintType.Metallic;

        // Act
        color.UpdateDetails(newName, newHexColor, newBottleSize, newType, line);

        // Assert
        color.Name.ShouldBe(newName);
        color.HexColor.ShouldBe(newHexColor);
        color.BottleSize.ShouldBe(newBottleSize);
        color.Type.ShouldBe(newType);
    }

    [Test]
    public void UpdateDetails_Can_Update_ManufacturerCode()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");
        var color = new PaintColor(line, "Test Paint", "#000000", 12.0, PaintType.Opaque);

        var manufacturerCode = "21-25";

        // Act
        color.UpdateDetails("Test Paint", "#000000", 12.0, PaintType.Opaque, line, manufacturerCode);

        // Assert
        color.ManufacturerCode.ShouldBe(manufacturerCode);
    }

    [Test]
    public void UpdateDetails_Can_Change_Line()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var originalLine = new PaintLine(brand, "Base");
        var newLine = new PaintLine(brand, "Layer");
        var color = new PaintColor(originalLine, "Test Paint", "#000000", 12.0, PaintType.Opaque);

        // Act
        color.UpdateDetails("Test Paint", "#000000", 12.0, PaintType.Opaque, newLine);

        // Assert
        color.Line.ShouldBe(newLine);
        color.LineId.ShouldBe(newLine.Id);
    }

    [Test]
    public void UpdateDetails_Can_Remove_ManufacturerCode()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");
        var color = new PaintColor(line, "Test Paint", "#000000", 12.0, PaintType.Opaque, "21-25");

        // Act
        color.UpdateDetails("Test Paint", "#000000", 12.0, PaintType.Opaque, line, null);

        // Assert
        color.ManufacturerCode.ShouldBeNull();
    }

    [Test]
    public void UpdateDetails_Can_Be_Called_Multiple_Times()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");
        var color = new PaintColor(line, "Test Paint", "#000000", 12.0, PaintType.Opaque);

        // Act
        color.UpdateDetails("Name 1", "#111111", 13.0, PaintType.Metallic, line);
        color.UpdateDetails("Name 2", "#222222", 14.0, PaintType.Wash, line);
        color.UpdateDetails("Name 3", "#333333", 15.0, PaintType.Contrast, line);

        // Assert
        color.Name.ShouldBe("Name 3");
        color.HexColor.ShouldBe("#333333");
        color.BottleSize.ShouldBe(15.0);
        color.Type.ShouldBe(PaintType.Contrast);
    }

    [Test]
    public void Id_Is_Generated_As_Empty_Guid_Before_Database_Persistence()
    {
        // Arrange
        var brand = new PaintBrand("Citadel");
        var line = new PaintLine(brand, "Base");

        // Act
        var color = new PaintColor(line, "Test Paint", "#000000", 12.0, PaintType.Opaque);

        // Assert
        // The Id will be Guid.Empty until EF Core generates it during SaveChanges
        color.Id.ShouldBe(Guid.Empty);
    }
}
