using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastHour.Api.Tests.Endpoints;

/// <summary>
/// Exercises the API host end to end to validate that the composition root starts and
/// the system endpoints respond as contracted.
/// </summary>
[Collection("ApiEndpoints")]
public class SystemEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SystemEndpointsTests"/> class.
    /// </summary>
    /// <param name="factory">The shared API host factory provided by xUnit.</param>
    public SystemEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Root_ReturnsServiceInfo()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        ServiceInfoResponse? payload = await response.Content.ReadFromJsonAsync<ServiceInfoResponse>();
        Assert.NotNull(payload);
        Assert.Equal("LastHour.Api", payload.Service);
        Assert.Equal("ready", payload.Status);
    }

    [Fact]
    public async Task Get_Health_ReturnsOk()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_Liveness_ReturnsOk()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_SwaggerDocument_ReturnsJsonInDevelopment()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.Content.Headers.ContentType?.ToString());
    }

    /// <summary>
    /// Describes the root endpoint payload.
    /// </summary>
    /// <param name="Service">The name of the service.</param>
    /// <param name="Status">The operational status of the service.</param>
    private sealed record ServiceInfoResponse(string Service, string Status);
}
