using PaintingProjectsManagement.UI.Modules.Projects;

namespace PaintingProjectsManagement.Features.Projects.UI;

public class MaterialDetails
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public MoneyDetails PackagePrice { get; init; } = MoneyDetails.Empty;
    public QuantityDetails PackageContent { get; init; } = QuantityDetails.Empty;
}