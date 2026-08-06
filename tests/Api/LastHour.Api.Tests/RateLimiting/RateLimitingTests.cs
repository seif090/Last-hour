using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastHour.Api.Tests.RateLimiting;

/// <summary>
/// Exercises the rate limiting middleware end to end: the global limiter applied to every request
/// and the per-endpoint limiter applied to the system status endpoint both reject requests that
/// exceed their configured budget with a problem details response.
/// </summary>
[Collection("ApiEndpoints")]
public class RateLimitingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RateLimitingTests"/> class.
    /// </summary>
    /// <param name="factory">The shared API host factory provided by xUnit.</param>
    public RateLimitingTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Root_ExceedingGlobalLimit_ReturnsTooManyRequests()
    {
        using WebApplicationFactory<Program> factory = CreateFactory(new Dictionary<string, string?>
        {
            ["RateLimiting:Policies:0:PermitLimit"] = "2",
        });

        HttpClient client = factory.CreateClient();

        HttpResponseMessage first = await client.GetAsync("/");
        HttpResponseMessage second = await client.GetAsync("/");
        HttpResponseMessage third = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
        Assert.Equal("application/problem+json", third.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Get_SystemStatus_ExceedingEndpointLimit_ReturnsTooManyRequests()
    {
        using WebApplicationFactory<Program> factory = CreateFactory(new Dictionary<string, string?>
        {
            ["RateLimiting:Policies:1:PermitLimit"] = "2",
        });

        HttpClient client = factory.CreateClient();

        HttpResponseMessage first = await client.GetAsync("/api/v1/system/status");
        HttpResponseMessage second = await client.GetAsync("/api/v1/system/status");
        HttpResponseMessage third = await client.GetAsync("/api/v1/system/status");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(HttpStatusCode.TooManyRequests, third.StatusCode);
    }

    private WebApplicationFactory<Program> CreateFactory(Dictionary<string, string?> overrides)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            foreach (KeyValuePair<string, string?> setting in overrides)
            {
                builder.UseSetting(setting.Key, setting.Value);
            }
        });
    }
}
