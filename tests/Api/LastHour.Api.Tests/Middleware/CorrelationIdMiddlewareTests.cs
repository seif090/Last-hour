using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastHour.Api.Tests.Middleware;

/// <summary>
/// Exercises the correlation id middleware: it honors an incoming header, generates a new id
/// when none is supplied, echoes the id on every response and surfaces it in error responses.
/// </summary>
[Collection("ApiEndpoints")]
public class CorrelationIdMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationIdMiddlewareTests"/> class.
    /// </summary>
    /// <param name="factory">The shared API host factory provided by xUnit.</param>
    public CorrelationIdMiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_WithCorrelationHeader_ReturnsSameCorrelationId()
    {
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-ID", "corr-abc-123");

        HttpResponseMessage response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("corr-abc-123", response.Headers.GetValues("X-Correlation-ID").Single());
    }

    [Fact]
    public async Task Get_WithoutCorrelationHeader_GeneratesCorrelationId()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string correlationId = Assert.Single(response.Headers.GetValues("X-Correlation-ID"));
        Assert.False(string.IsNullOrWhiteSpace(correlationId));
    }

    [Fact]
    public async Task Get_UnhandledError_ReturnsCorrelationIdInProblemDetails()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/system/problems/unhandled");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        string headerCorrelationId = Assert.Single(response.Headers.GetValues("X-Correlation-ID"));
        Assert.False(string.IsNullOrWhiteSpace(headerCorrelationId));

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal(headerCorrelationId, document.RootElement.GetProperty("correlationId").GetString());
    }
}
