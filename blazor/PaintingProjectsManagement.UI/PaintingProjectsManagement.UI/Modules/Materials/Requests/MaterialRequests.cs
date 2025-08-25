namespace PaintingProjectsManagement.UI.Modules.Materials;

// DOCS: Simple crud requests for create/update that don't differ much such be in the same file
public class CreateMaterialRequest
{
    public string Name { get; init; } = string.Empty;
    public double PackageContentAmount { get; init; }
    public PackageContentUnit PackageContentUnit { get; init; } = PackageContentUnit.Gram;
    public double PackagePriceAmount { get; init; }
    public string PackagePriceCurrency { get; init; } = string.Empty;
}

public class UpdateMaterialRequest : CreateMaterialRequest
{
    public Guid Id { get; init; }
}
