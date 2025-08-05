using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.Blazor.Modules.Shared;

public class EnumReference
{
    [JsonConstructor]
    private EnumReference() 
    {
    }

    public EnumReference(int id, string value)
    {
        Id = id;
        Value = value;
    }

    public EnumReference(Enum value)
    {
        Id = Convert.ToInt32(value);
        Value = value.ToString();
    }

    public int Id { get; set; }
    public string Value { get; set; } = string.Empty;
}