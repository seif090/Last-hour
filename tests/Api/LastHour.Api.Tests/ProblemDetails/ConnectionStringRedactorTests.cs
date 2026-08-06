using LastHour.Api.ProblemDetails;

namespace LastHour.Api.Tests.ProblemDetails;

/// <summary>
/// Exercises the connection string and credential redaction applied to exception detail.
/// </summary>
public class ConnectionStringRedactorTests
{
    [Fact]
    public void Redact_PasswordPair_ReplacesSecretValue()
    {
        string redacted = ConnectionStringRedactor.Redact("Server=db;Password=hunter2");

        Assert.DoesNotContain("hunter2", redacted);
        Assert.Contains("Password=***", redacted);
    }

    [Fact]
    public void Redact_ConnectionStringReference_RemovesEntireValue()
    {
        string redacted = ConnectionStringRedactor.Redact(
            "Failed. ConnectionString: Server=db-01;User Id=sa;Password=hunter2;TrustServerCertificate=True");

        Assert.DoesNotContain("hunter2", redacted);
        Assert.DoesNotContain("db-01", redacted);
        Assert.DoesNotContain("TrustServerCertificate", redacted);
        Assert.Contains("connection string: ***", redacted);
    }

    [Fact]
    public void Redact_CommonCredentialKeys_AreRedacted()
    {
        string redacted = ConnectionStringRedactor.Redact("ApiKey=abc123 Secret=xyz Pwd=topsecret");

        Assert.DoesNotContain("abc123", redacted);
        Assert.DoesNotContain("xyz", redacted);
        Assert.DoesNotContain("topsecret", redacted);
    }

    [Fact]
    public void Redact_PlainText_ReturnsUnchanged()
    {
        const string message = "A demonstration unhandled exception.";

        Assert.Equal(message, ConnectionStringRedactor.Redact(message));
    }

    [Fact]
    public void Redact_NullOrBlank_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, ConnectionStringRedactor.Redact(null));
        Assert.Equal(string.Empty, ConnectionStringRedactor.Redact("   "));
    }
}
