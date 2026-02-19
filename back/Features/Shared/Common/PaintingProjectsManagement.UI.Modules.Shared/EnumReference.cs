using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.UI.Modules.Shared;

public record EnumReference
{

    [JsonConstructor]
    public EnumReference(int id, string value)
    {
        Id = id;
        Value = value;
    }

    public int Id { get; init; }
    public string Value { get; init; }

    public  static EnumReference FromValue(Enum value)
    {
        return new EnumReference(Convert.ToInt32(value), value.ToString() ?? string.Empty);
    }
}

public record EntityReference(Guid Id, string Name);