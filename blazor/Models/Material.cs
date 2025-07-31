namespace PaintingProjectsManagement.Blazor.Models;

public class Material
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public MaterialUnit Unit { get; set; } = new();
    public double PricePerUnit { get; set; }
}

public class MaterialUnit
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;
} 