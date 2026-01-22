namespace PaintingProjectsManagement.Features.Inventory.Integration;

public interface IFindColorMatchesCommand
{
    string ReferenceColor { get; } // Hex color
    int MaxResults { get; }
    string Tenant { get; } // User tenant for filtering paints
}
