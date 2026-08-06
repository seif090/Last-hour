using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastHour.Api.Tests.Endpoints;

/// <summary>
/// Exercises the URL segment API versioning contract and the per-version Swagger documents.
/// </summary>
[Collection("ApiEndpoints")]
public class ApiVersioningTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ApiVersioningTests"/> class.
    /// </summary>
    /// <param name="factory">The shared API host factory provided by xUnit.</param>
    public ApiVersioningTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_VersionedStatus_V1_ReturnsServedVersion()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/system/status");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(response.Headers.Contains("api-supported-versions"));
        SystemStatusResponse? payload = await response.Content.ReadFromJsonAsync<SystemStatusResponse>();
        Assert.NotNull(payload);
        Assert.Equal("v1", payload!.Version);
    }

    [Fact]
    public async Task Get_VersionedStatus_V2_ReturnsNotFound()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v2/system/status");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_VersionedStatus_UnsupportedVersion_ReturnsNotFound()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v4/system/status");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_SwaggerDocument_V1_ContainsVersionedPathSecurityAndCorrelationHeader()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/api/v1/system/status", out JsonElement statusOperation));

        JsonElement components = document.RootElement.GetProperty("components");
        Assert.True(components.GetProperty("securitySchemes").TryGetProperty("Bearer", out _));

        JsonElement security = document.RootElement.GetProperty("security");
        Assert.Contains(security.EnumerateArray(), scheme => scheme.TryGetProperty("Bearer", out _));

        JsonElement operation = statusOperation.GetProperty("get");
        JsonElement parameters = operation.GetProperty("parameters");
        Assert.Contains(parameters.EnumerateArray(), parameter => parameter.GetProperty("name").GetString() == "X-Correlation-ID");
    }

    [Fact]
    public async Task Get_SwaggerDocument_V2_OmitsUnsupportedVersionedPath()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/swagger/v2/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement paths = document.RootElement.GetProperty("paths");
        Assert.False(paths.TryGetProperty("/api/v2/system/status", out _));
    }

    [Fact]
    public async Task Get_SwaggerDocument_V3_ReturnsJsonInDevelopment()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/swagger/v3/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    /// <summary>
    /// Describes the versioned status endpoint payload.
    /// </summary>
    /// <param name="Version">The API version that served the request.</param>
    private sealed record SystemStatusResponse(string Version);
}
