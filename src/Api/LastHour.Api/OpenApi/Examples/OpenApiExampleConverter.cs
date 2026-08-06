using System.Text.Json;
using Microsoft.OpenApi.Any;

namespace LastHour.Api.OpenApi.Examples;

/// <summary>
/// Converts example values into the Microsoft.OpenApi value model that Swagger documents expect.
/// Values are serialized with System.Text.Json first, so examples match the exact wire format the
/// API produces.
/// </summary>
public static class OpenApiExampleConverter
{
    /// <summary>
    /// Converts the given value into an OpenAPI example value.
    /// </summary>
    /// <typeparam name="T">The runtime type of the example value.</typeparam>
    /// <param name="value">The value to convert.</param>
    /// <returns>The OpenAPI representation of the value.</returns>
    public static IOpenApiAny ToOpenApiAny<T>(T value) =>
        FromJsonElement(JsonSerializer.SerializeToElement(value));

    private static IOpenApiAny FromJsonElement(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                return ToObject(element);
            case JsonValueKind.Array:
                return ToArray(element);
            case JsonValueKind.String:
                return new OpenApiString(element.GetString() ?? string.Empty);
            case JsonValueKind.Number:
                return element.TryGetInt64(out long integral)
                    ? new OpenApiLong(integral)
                    : new OpenApiDouble(element.GetDouble());
            case JsonValueKind.True:
                return new OpenApiBoolean(true);
            case JsonValueKind.False:
                return new OpenApiBoolean(false);
            case JsonValueKind.Null:
                return new OpenApiNull();
            default:
                return new OpenApiString(element.GetRawText());
        }
    }

    private static OpenApiArray ToArray(JsonElement element)
    {
        var openApiArray = new OpenApiArray();
        openApiArray.AddRange(element.EnumerateArray().Select(FromJsonElement));
        return openApiArray;
    }

    private static OpenApiObject ToObject(JsonElement element)
    {
        var openApiObject = new OpenApiObject();

        foreach (JsonProperty property in element.EnumerateObject())
        {
            openApiObject.Add(property.Name, FromJsonElement(property.Value));
        }

        return openApiObject;
    }
}
