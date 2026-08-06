using Asp.Versioning;

namespace LastHour.Api.Versioning;

/// <summary>
/// Declares the API versions supported by the LastHour API. Adding a future version
/// means extending this table and registering any endpoints that serve it; the versioning
/// and OpenAPI infrastructure picks the rest up from here.
/// </summary>
public static class ApiVersions
{
    /// <summary>
    /// Gets version 1.0, the current, default version of the API.
    /// </summary>
    public static ApiVersion V1 { get; } = new ApiVersion(1, 0);

    /// <summary>
    /// Gets version 2.0, reserved for the next breaking change.
    /// </summary>
    public static ApiVersion V2 { get; } = new ApiVersion(2, 0);

    /// <summary>
    /// Gets version 3.0, reserved for the version after next.
    /// </summary>
    public static ApiVersion V3 { get; } = new ApiVersion(3, 0);

    /// <summary>
    /// Gets the versions served by this API, in ascending order.
    /// </summary>
    public static IReadOnlyList<ApiVersion> Supported { get; } = new[] { V1, V2, V3 };
}
