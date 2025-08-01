using PaintingProjectsManagement.UI.Client.Models;

namespace PaintingProjectsManagement.UI.Client.Modules.Materials.Models;

public class MaterialDetails
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public EnumReference Unit { get; set; }
    public double PricePerUnit { get; set; }
}

public class EnumReference
{
    public int Id { get; set; }
    public string Value{ get; set; } = string.Empty;

    public EnumReference() { }

    public EnumReference(int id, string value)
    {
        Id = id;    
        Value = value;
    }
} 