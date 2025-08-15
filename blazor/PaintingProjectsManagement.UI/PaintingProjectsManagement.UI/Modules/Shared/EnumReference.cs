using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.UI.Modules.Shared;

public class EnumReference
{
    [JsonConstructor]
    private EnumReference()
    {
    }

    public EnumReference(int id, string value)
    {
        this.Id = id;
        this.Value = value;
    }

    public EnumReference(Enum value)
    {
        this.Id = Convert.ToInt32((object)value);
        this.Value = value.ToString();
    }

    public int Id { get; set; }

    public string Value { get; set; } = string.Empty;

    public override string ToString() => this.Value;
}