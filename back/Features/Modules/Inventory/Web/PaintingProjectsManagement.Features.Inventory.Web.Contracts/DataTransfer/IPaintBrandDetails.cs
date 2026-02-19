namespace PaintingProjectsManagement.Features.Inventory.Web;

public interface IPaintBrandDetails
{
    Guid Id { get; }
    string Name { get; }
    IReadOnlyList<IPaintLineDetails> Lines { get; }
}