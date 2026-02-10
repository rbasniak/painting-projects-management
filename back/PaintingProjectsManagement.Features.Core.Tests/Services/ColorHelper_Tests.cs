namespace PaintingProjectsManagement.Features.Inventory.Core.Tests;

public class ColorHelper_Tests
{
    [Test]
    public void CalculateColorDistance_Returns_Zero_For_Identical_Colors()
    {
        // Arrange
        var color1 = "#FF0000";
        var color2 = "#FF0000";

        // Act
        var distance = ColorHelper.CalculateColorDistance(color1, color2);

        // Assert
        distance.ShouldBe(0);
    }

    [Test]
    public void CalculateColorDistance_Calculates_Correct_Distance_Between_Red_And_Blue()
    {
        // Arrange
        var red = "#FF0000";
        var blue = "#0000FF";

        // Act
        var distance = ColorHelper.CalculateColorDistance(red, blue);

        // Assert
        distance.ShouldBe(360.62445840513925, tolerance: 0.0001);
    }

    [Test]
    public void CalculateColorDistance_Calculates_Correct_Distance_Between_Red_And_Green()
    {
        // Arrange
        var red = "#FF0000";
        var green = "#00FF00";

        // Act
        var distance = ColorHelper.CalculateColorDistance(red, green);

        // Assert
        distance.ShouldBe(360.62445840513925, tolerance: 0.0001);
    }

    [Test]
    public void CalculateColorDistance_Works_With_Colors_Without_Hash()
    {
        // Arrange
        var color1 = "FF0000";
        var color2 = "0000FF";

        // Act
        var distance = ColorHelper.CalculateColorDistance(color1, color2);

        // Assert
        distance.ShouldBe(360.62445840513925, tolerance: 0.0001);
    }

    [Test]
    public void CalculateColorDistance_Throws_Exception_For_Invalid_Hex_Format()
    {
        // Arrange
        var invalidColor = "#FFF";
        var validColor = "#FF0000";

        // Act & Assert
        Should.Throw<ArgumentException>(() => ColorHelper.CalculateColorDistance(invalidColor, validColor));
    }

    [Test]
    public void CalculateColorDistance_Calculates_Correct_Distance_For_Similar_Colors()
    {
        // Arrange
        var color1 = "#FF0000";
        var color2 = "#FE0000";

        // Act
        var distance = ColorHelper.CalculateColorDistance(color1, color2);

        // Assert
        distance.ShouldBe(1.0, tolerance: 0.0001);
    }

    [Test]
    public void CalculateColorSimilarity_Returns_One_For_Identical_Colors()
    {
        // Arrange
        var color1 = "#FF0000";
        var color2 = "#FF0000";

        // Act
        var similarity = ColorHelper.CalculateColorSimilarity(color1, color2);

        // Assert
        similarity.ShouldBe(1.0);
    }

    [Test]
    public void CalculateColorSimilarity_Returns_Low_Value_For_Opposite_Colors()
    {
        // Arrange
        var black = "#000000";
        var white = "#FFFFFF";

        // Act
        var similarity = ColorHelper.CalculateColorSimilarity(black, white);

        // Assert
        similarity.ShouldBe(0.0, tolerance: 0.0001);
    }

    [Test]
    public void CalculateColorSimilarity_Returns_High_Value_For_Similar_Colors()
    {
        // Arrange
        var color1 = "#FF0000";
        var color2 = "#FE0000";

        // Act
        var similarity = ColorHelper.CalculateColorSimilarity(color1, color2);

        // Assert
        similarity.ShouldBeGreaterThan(0.99);
    }

    [Test]
    public void CalculateColorSimilarity_Returns_Value_Between_Zero_And_One()
    {
        // Arrange
        var color1 = "#FF0000";
        var color2 = "#00FF00";

        // Act
        var similarity = ColorHelper.CalculateColorSimilarity(color1, color2);

        // Assert
        similarity.ShouldBeInRange(0.0, 1.0);
    }

    [Test]
    public void CalculateColorDistance_Works_With_Gray_Scale_Colors()
    {
        // Arrange
        var black = "#000000";
        var gray = "#808080";

        // Act
        var distance = ColorHelper.CalculateColorDistance(black, gray);

        // Assert
        distance.ShouldBe(221.70250102042982, tolerance: 0.0001);
    }

    [Test]
    [Arguments("#FF0000", "#FF0000", 0.0)]
    [Arguments("#000000", "#FFFFFF", 441.6729559300637)]
    [Arguments("#FF0000", "#00FF00", 360.62445840513925)]
    [Arguments("#FF0000", "#0000FF", 360.62445840513925)]
    public void CalculateColorDistance_Returns_Expected_Values(string color1, string color2, double expectedDistance)
    {
        // Act
        var distance = ColorHelper.CalculateColorDistance(color1, color2);

        // Assert
        distance.ShouldBe(expectedDistance, tolerance: 0.0001);
    }
}
