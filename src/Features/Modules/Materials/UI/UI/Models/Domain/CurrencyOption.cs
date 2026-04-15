namespace PaintingProjectsManagement.UI.Modules.Materials;

public record CurrencyOption
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Label => string.IsNullOrWhiteSpace(Name) ? Code : $"{Code} - {Name}";
}
