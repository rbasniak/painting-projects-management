namespace PaintingProjectsManagement.UI.Client.Modules.Materials.Models;

public class MaterialDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EnumReference Unit { get; set; } = new();
    public double PricePerUnit { get; set; }
}

public class EnumReference
{
    public int Value { get; set; }
    public string Name { get; set; } = string.Empty;

    public EnumReference() { }

    public EnumReference(int value, string name)
    {
        Value = value;
        Name = name;
    }
} 