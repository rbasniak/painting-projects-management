namespace rbkApiModules.Commons.Core.Features.ApplicationOptions;

public class ApplicationOption
{
    private ApplicationOption()
    {
        // EF Core constructor, don't remove it
    }   

    public ApplicationOption(string key, string value)
    {
        Key = key;
        Value = value;
    }

    public string Key { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
}
