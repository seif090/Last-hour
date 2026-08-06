using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace LastHour.Api.Tests.Endpoints;

/// <summary>
/// Exercises the RFC 7807 problem details contract end to end: failed results are converted
/// into problem details responses automatically, and unhandled exceptions become a 500 problem
/// details response without leaking internal details.
/// </summary>
[Collection("ApiEndpoints")]
public class ProblemDetailsEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemDetailsEndpointsTests"/> class.
    /// </summary>
    /// <param name="factory">The shared API host factory provided by xUnit.</param>
    public ProblemDetailsEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Get_Validation_ReturnsBadRequestProblemDetails()
    {
        ProblemResponse problem = await GetProblemAsync("validation");

        Assert.Equal(HttpStatusCode.BadRequest, problem.Status);
        Assert.Equal("application/problem+json", problem.ContentType);
        Assert.Equal(400, problem.Body.GetProperty("status").GetInt32());
        Assert.Equal("Bad Request", problem.Body.GetProperty("title").GetString());
        Assert.Equal("https://lasthour.dev/errors/validation", problem.Body.GetProperty("type").GetString());
        Assert.Equal("One or more validation errors occurred.", problem.Body.GetProperty("detail").GetString());
        Assert.Equal("ValidationFailed", problem.Body.GetProperty("code").GetString());

        JsonElement errors = problem.Body.GetProperty("errors");
        Assert.True(errors.TryGetProperty("Email", out JsonElement email));
        Assert.Equal("'Email' is required.", email[0].GetString());
        Assert.True(errors.TryGetProperty("Password", out JsonElement password));
        Assert.Equal("'Password' must be at least 8 characters.", password[0].GetString());
    }

    [Fact]
    public async Task Get_NotFound_ReturnsNotFoundProblemDetails()
    {
        ProblemResponse problem = await GetProblemAsync("not-found");

        Assert.Equal(HttpStatusCode.NotFound, problem.Status);
        Assert.Equal(404, problem.Body.GetProperty("status").GetInt32());
        Assert.Equal("Not Found", problem.Body.GetProperty("title").GetString());
        Assert.Equal("https://lasthour.dev/errors/notfound", problem.Body.GetProperty("type").GetString());
        Assert.Equal("ResourceNotFound", problem.Body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_Conflict_ReturnsConflictProblemDetails()
    {
        ProblemResponse problem = await GetProblemAsync("conflict");

        Assert.Equal(HttpStatusCode.Conflict, problem.Status);
        Assert.Equal(409, problem.Body.GetProperty("status").GetInt32());
        Assert.Equal("Conflict", problem.Body.GetProperty("title").GetString());
        Assert.Equal("ResourceConflict", problem.Body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_Unauthorized_ReturnsUnauthorizedProblemDetails()
    {
        ProblemResponse problem = await GetProblemAsync("unauthorized");

        Assert.Equal(HttpStatusCode.Unauthorized, problem.Status);
        Assert.Equal(401, problem.Body.GetProperty("status").GetInt32());
        Assert.Equal("Unauthorized", problem.Body.GetProperty("title").GetString());
        Assert.Equal("Unauthorized", problem.Body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_Forbidden_ReturnsForbiddenProblemDetails()
    {
        ProblemResponse problem = await GetProblemAsync("forbidden");

        Assert.Equal(HttpStatusCode.Forbidden, problem.Status);
        Assert.Equal(403, problem.Body.GetProperty("status").GetInt32());
        Assert.Equal("Forbidden", problem.Body.GetProperty("title").GetString());
        Assert.Equal("Forbidden", problem.Body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_Unhandled_ReturnsInternalServerErrorProblemDetails()
    {
        ProblemResponse problem = await GetProblemAsync("unhandled");

        Assert.Equal(HttpStatusCode.InternalServerError, problem.Status);
        Assert.Equal("application/problem+json", problem.ContentType);
        Assert.Equal(500, problem.Body.GetProperty("status").GetInt32());
        Assert.Equal("Internal Server Error", problem.Body.GetProperty("title").GetString());
        Assert.Equal("https://lasthour.dev/errors/failure", problem.Body.GetProperty("type").GetString());
        Assert.Equal("UnhandledException", problem.Body.GetProperty("code").GetString());
        Assert.Contains("A demonstration unhandled exception.", problem.Body.GetProperty("detail").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.CorrelationId));
        Assert.Equal(problem.CorrelationId, problem.Body.GetProperty("correlationId").GetString());
    }

    private async Task<ProblemResponse> GetProblemAsync(string kind)
    {
        HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync($"/api/v1/system/problems/{kind}");

        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return new ProblemResponse(
            response.StatusCode,
            response.Content.Headers.ContentType?.MediaType,
            document.RootElement.Clone(),
            response.Headers.TryGetValues("X-Correlation-ID", out IEnumerable<string>? values)
                ? values.Single()
                : null);
    }

    /// <summary>
    /// Carries the response surface of a problem details endpoint call.
    /// </summary>
    /// <param name="Status">The HTTP status code returned.</param>
    /// <param name="ContentType">The media type of the response body.</param>
    /// <param name="Body">The parsed problem details JSON.</param>
    /// <param name="CorrelationId">The correlation id echoed on the response.</param>
    private sealed record ProblemResponse(HttpStatusCode Status, string? ContentType, JsonElement Body, string? CorrelationId);
}
