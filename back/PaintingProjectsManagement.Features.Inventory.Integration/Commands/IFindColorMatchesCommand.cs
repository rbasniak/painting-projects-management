namespace PaintingProjectsManagement.Features.Inventory.Integration;

public interface IFindColorMatchesCommand: IQuery<IReadOnlyCollection<ColorMatchResult>>
{
    string ReferenceColor { get; }  
    int MaxResults { get; }
}

public class FindColorMatchesCommandRequest : AuthenticatedRequest, IFindColorMatchesCommand
{
    public string ReferenceColor { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 10;
}
