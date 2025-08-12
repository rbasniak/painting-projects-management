using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace rbkApiModules.Commons.Core;

public static class JsonEventSerializer
{
    // One place to centralize options
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        // add/adjust converters here (e.g., enums as strings)
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = false
    };

    public static JsonSerializerOptions GetOptions() => Options;

    public static string Serialize<T>(EventEnvelope<T> envelope) =>
        JsonSerializer.Serialize(envelope, Options);

    public static EventEnvelope<T> Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<EventEnvelope<T>>(json, Options)!;

    // New: generic type overload for runtime Type
    public static object Deserialize(string json, Type targetType) =>
        JsonSerializer.Deserialize(json, targetType, Options)!;
} 