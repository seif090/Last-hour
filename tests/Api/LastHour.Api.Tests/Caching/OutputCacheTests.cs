using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastHour.Api.Tests.Caching;

/// <summary>
/// Exercises the output cache middleware end to end: the root endpoint opts into the
/// <c>Default</c> cache profile, so repeated requests are served from cache, and requests that
/// carry credentials are never cached by the framework default policy.
/// </summary>
[Collection("ApiEndpoints")]
public class OutputCacheTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputCacheTests"/> class.
    /// </summary>
    /// <param name="factory">The shared API host factory provided by xUnit.</param>
    public OutputCacheTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Root_SecondRequest_IsServedFromCache()
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage first = await client.GetAsync("/");
        HttpResponseMessage second = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.False(first.Headers.Contains("Age"), "The first request must miss the cache.");
        Assert.True(second.Headers.Contains("Age"), "The second request must be served from cache.");
    }

    [Fact]
    public async Task Get_Root_WithAuthorizationHeader_IsNeverCached()
    {
        HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "test-token");

        HttpResponseMessage first = await client.GetAsync("/");
        HttpResponseMessage second = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.False(first.Headers.Contains("Age"), "Credentialed requests must never be served from cache.");
        Assert.False(second.Headers.Contains("Age"), "Credentialed requests must never be served from cache.");
    }
}
