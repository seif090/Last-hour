using System.Net;
using LastHour.Api.Tests.Support;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastHour.Api.Tests.Middleware;

/// <summary>
/// Verifies the security headers emitted by the API host on every response.
/// </summary>
[Collection("ApiEndpoints")]
public class SecurityHeadersTests : IClassFixture<LastHourApiFactory>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityHeadersTests"/> class.
    /// </summary>
    /// <param name="factory">The shared API host factory provided by xUnit.</param>
    public SecurityHeadersTests(LastHourApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Root_ReturnsSecurityHeaders()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("strict-origin-when-cross-origin", response.Headers.GetValues("Referrer-Policy").Single());
        Assert.Contains("frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single());
        Assert.True(response.Headers.Contains("Permissions-Policy"));
    }

    [Fact]
    public async Task Get_Root_DoesNotEmitXssProtectionOverHttp()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("X-XSS-Protection"));
    }

    [Fact]
    public async Task Get_Root_DoesNotEmitHstsOverHttp()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(response.Headers.Contains("Strict-Transport-Security"));
    }
}
