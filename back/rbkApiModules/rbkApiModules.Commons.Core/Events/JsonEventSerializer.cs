using System.Text.Json;
using System.Text.Json.Serialization;

namespace rbkApiModules.Commons.Core;

public static class JsonEventSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Serialize<TEvent>(EventEnvelope<TEvent> envelope)
    {
        return JsonSerializer.Serialize(envelope, Options);
    }

    public static EventEnvelope<TEvent> Deserialize<TEvent>(string json)
    {
        return JsonSerializer.Deserialize<EventEnvelope<TEvent>>(json, Options)!;
    }
} 