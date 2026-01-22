namespace PaintingProjectsManagement.Features.Inventory;

public class UserPaint : BaseEntity
{
    private UserPaint() { }

    public UserPaint(string username, Guid paintColorId)
    {
        Username = username;
        PaintColorId = paintColorId;
    }

    public string Username { get; private set; } = string.Empty;
    public Guid PaintColorId { get; private set; }
    public PaintColor PaintColor { get; private set; } = null!;
}
