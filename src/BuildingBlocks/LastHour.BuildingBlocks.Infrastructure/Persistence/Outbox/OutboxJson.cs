using System.Text.Json;
using System.Text.Json.Serialization;
using LastHour.BuildingBlocks.Infrastructure.StronglyTypedIds;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Outbox;

/// <summary>
/// Centralizes the JSON serialization settings used to persist and rehydrate outbox messages.
/// </summary>
public static class OutboxJson
{
    /// <summary>
    /// Gets the serializer options shared by the outbox writer and processor. Properties are
    /// serialized as camel case and write semantics use the runtime type, so polymorphic
    /// events round-trip correctly. Strongly typed identifiers are serialized as their
    /// primitive value so events carrying identifiers survive the round-trip.
    /// </summary>
    public static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new StronglyTypedIdJsonConverterFactory() },
    };
}
