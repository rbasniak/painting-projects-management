using System.Text.Json.Serialization;

namespace PaintingProjectsManagement.UI.Modules.Shared;

public record EnumReference
{

    public EnumReference(Enum value)
    {
        Id = Convert.ToInt32(value);
        Value = value.ToString();
    }

    [JsonConstructor]
    public EnumReference(int id, string value)
    {
        Id = id;
        Value = value;
    }

    public int Id { get; init; }
    public string Value { get; init; }
}

public record EntityReference(Guid Id, string Name);