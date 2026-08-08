using System.Net;
using LastHour.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastHour.Api.Tests.Middleware;

/// <summary>
/// Verifies the CORS surface of the API host. The test host runs in the Development environment,
/// where any origin is allowed, so cross-origin requests and preflights succeed.
/// </summary>
[Collection("ApiEndpoints")]
public class CorsTests : IClassFixture<LastHourApiFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorsTests"/> class.
    /// </summary>
    /// <param name="factory">The shared API host factory provided by xUnit.</param>
    public CorsTests(LastHourApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_WithOrigin_ReturnsAllowOriginHeader()
    {
        HttpClient client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("Origin", "https://example.com");

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("*", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task Options_Preflight_ReturnsNoContentWithCorsHeaders()
    {
        HttpClient client = _factory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Options, "/");
        request.Headers.Add("Origin", "https://example.com");
        request.Headers.TryAddWithoutValidation("Access-Control-Request-Method", "GET");

        HttpResponseMessage response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("*", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
        Assert.Contains("GET", response.Headers.GetValues("Access-Control-Allow-Methods").Single());
    }
}
