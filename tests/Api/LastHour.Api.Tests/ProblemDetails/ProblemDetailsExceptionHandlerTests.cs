using System.Text.Json;
using LastHour.Api.Middleware;
using LastHour.Api.ProblemDetails;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace LastHour.Api.Tests.ProblemDetails;

/// <summary>
/// Exercises the exception handler's response contract: unhandled exceptions become a 500
/// problem details response that never leaks a stack trace or connection string and always
/// carries the request correlation id.
/// </summary>
public class ProblemDetailsExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_Production_ReturnsGenericResponseWithoutSensitiveDetails()
    {
        var environment = new StubEnvironment { EnvironmentName = Environments.Production };
        var handler = new ProblemDetailsExceptionHandler(NullLogger<ProblemDetailsExceptionHandler>.Instance, environment);
        using var body = new MemoryStream();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = body;
        httpContext.Items[CorrelationIdDefaults.ContextKey] = "corr-123";
        Exception exception = ThrowWithConnectionString();

        bool handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
        Assert.Equal("application/problem+json", httpContext.Response.ContentType);

        string json = await ReadBodyAsync(body);
        Assert.DoesNotContain("hunter2", json);
        Assert.DoesNotContain("db-01", json);
        Assert.DoesNotContain("Request failed", json);
        Assert.DoesNotContain(" at ", json);
        Assert.Contains("An unexpected error occurred while processing your request.", json);

        using JsonDocument document = JsonDocument.Parse(json);
        Assert.Equal(500, document.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("UnhandledException", document.RootElement.GetProperty("code").GetString());
        Assert.Equal("corr-123", document.RootElement.GetProperty("correlationId").GetString());
    }

    [Fact]
    public async Task TryHandleAsync_Development_ReturnsRedactedMessageWithoutSecretsOrStack()
    {
        var environment = new StubEnvironment { EnvironmentName = Environments.Development };
        var handler = new ProblemDetailsExceptionHandler(NullLogger<ProblemDetailsExceptionHandler>.Instance, environment);
        using var body = new MemoryStream();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = body;
        Exception exception = ThrowWithConnectionString();

        bool handled = await handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        Assert.True(handled);
        string json = await ReadBodyAsync(body);
        Assert.Contains("Request failed", json);
        Assert.DoesNotContain("hunter2", json);
        Assert.DoesNotContain("db-01", json);
        Assert.DoesNotContain("TrustServerCertificate", json);
        Assert.DoesNotContain(" at ", json);
    }

    [Fact]
    public async Task TryHandleAsync_Cancellation_ReturnsHandledWithoutWriting()
    {
        var environment = new StubEnvironment { EnvironmentName = Environments.Production };
        var handler = new ProblemDetailsExceptionHandler(NullLogger<ProblemDetailsExceptionHandler>.Instance, environment);
        using var body = new MemoryStream();
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = body;

        bool handled = await handler.TryHandleAsync(httpContext, new OperationCanceledException(), CancellationToken.None);

        Assert.True(handled);
        Assert.Equal(StatusCodes.Status200OK, httpContext.Response.StatusCode);
        Assert.Equal(0, body.Length);
    }

    private static Exception ThrowWithConnectionString()
    {
        try
        {
            throw new InvalidOperationException(
                "Request failed. ConnectionString: Server=db-01;User Id=sa;Password=hunter2;TrustServerCertificate=True");
        }
        catch (Exception exception)
        {
            return exception;
        }
    }

    private static async Task<string> ReadBodyAsync(Stream body)
    {
        body.Position = 0;
        using StreamReader reader = new(body);
        return await reader.ReadToEndAsync();
    }

    private sealed class StubEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "LastHour.Api.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
