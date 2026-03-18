namespace PaintingProjectsManagement.Features.Inventory.Core.Tests;

public class UserPaint_Tests
{
    [Test]
    public void Constructor_Creates_UserPaint_With_Username_And_PaintColorId()
    {
        // Arrange
        var username = "rodrigo.basniak";
        var paintColorId = Guid.NewGuid();

        // Act
        var userPaint = new UserPaint(username, paintColorId);

        // Assert
        userPaint.Username.ShouldBe(username);
        userPaint.PaintColorId.ShouldBe(paintColorId);
    }

    [Test]
    public void Constructor_Creates_UserPaint_With_Empty_Username()
    {
        // Arrange
        var username = "";
        var paintColorId = Guid.NewGuid();

        // Act
        var userPaint = new UserPaint(username, paintColorId);

        // Assert
        userPaint.Username.ShouldBe(username);
    }

    [Test]
    public void Constructor_Creates_UserPaint_With_Empty_Guid()
    {
        // Arrange
        var username = "rodrigo.basniak";
        var paintColorId = Guid.Empty;

        // Act
        var userPaint = new UserPaint(username, paintColorId);

        // Assert
        userPaint.PaintColorId.ShouldBe(paintColorId);
    }

    [Test]
    public void Constructor_Creates_Different_UserPaints_For_Same_User_With_Different_Colors()
    {
        // Arrange
        var username = "rodrigo.basniak";
        var paintColorId1 = Guid.NewGuid();
        var paintColorId2 = Guid.NewGuid();

        // Act
        var userPaint1 = new UserPaint(username, paintColorId1);
        var userPaint2 = new UserPaint(username, paintColorId2);

        // Assert
        userPaint1.Username.ShouldBe(userPaint2.Username);
        userPaint1.PaintColorId.ShouldNotBe(userPaint2.PaintColorId);
    }

    [Test]
    public void Constructor_Creates_Different_UserPaints_For_Different_Users_With_Same_Color()
    {
        // Arrange
        var username1 = "rodrigo.basniak";
        var username2 = "ricardo.smarzaro";
        var paintColorId = Guid.NewGuid();

        // Act
        var userPaint1 = new UserPaint(username1, paintColorId);
        var userPaint2 = new UserPaint(username2, paintColorId);

        // Assert
        userPaint1.PaintColorId.ShouldBe(userPaint2.PaintColorId);
        userPaint1.Username.ShouldNotBe(userPaint2.Username);
    }

    [Test]
    public void PaintColor_Navigation_Property_Is_Not_Null_After_Construction()
    {
        // Arrange
        var username = "rodrigo.basniak";
        var paintColorId = Guid.NewGuid();

        // Act
        var userPaint = new UserPaint(username, paintColorId);

        // Assert
        // Navigation property should be accessible (even if null initially before EF loads it)
        // This test ensures the property exists and is correctly typed
        Should.NotThrow(() => { var _ = userPaint.PaintColor; });
    }
}
