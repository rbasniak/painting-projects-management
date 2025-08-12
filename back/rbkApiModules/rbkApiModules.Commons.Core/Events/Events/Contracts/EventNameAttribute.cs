namespace rbkApiModules.Commons.Core;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class EventNameAttribute : Attribute
{
    public EventNameAttribute(string name, int version = 1)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Version = version;
    }

    public string Name { get; }
    public int Version { get; }
} 