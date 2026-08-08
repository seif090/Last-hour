using LastHour.Api.Secrets;

namespace LastHour.Api.Tests.Secrets;

public class EnvironmentSecretProviderTests
{
    private const string Variable = "LASTHOUR_SECRET_CONNECTIONSTRINGS_POSTGRES";

    [Fact]
    public void GetSecret_WhenEnvironmentVariableExists_ReturnsValue()
    {
        string? previous = Environment.GetEnvironmentVariable(Variable);
        try
        {
            Environment.SetEnvironmentVariable(Variable, "secret-value");
            var provider = new EnvironmentSecretProvider(new SecretsOptions());

            string? value = provider.GetSecret("ConnectionStrings:Postgres");

            Assert.Equal("secret-value", value);
        }
        finally
        {
            Environment.SetEnvironmentVariable(Variable, previous);
        }
    }

    [Fact]
    public void GetSecret_WhenEnvironmentVariableMissing_ReturnsNull()
    {
        const string missing = "LASTHOUR_SECRET_DOESNOTEXIST_ABC123";
        string? previous = Environment.GetEnvironmentVariable(missing);
        try
        {
            Environment.SetEnvironmentVariable(missing, null);
            var provider = new EnvironmentSecretProvider(new SecretsOptions());

            string? value = provider.GetSecret("DoesNotExist:Abc123");

            Assert.Null(value);
        }
        finally
        {
            Environment.SetEnvironmentVariable(missing, previous);
        }
    }

    [Fact]
    public void GetSecret_WithCustomPrefix_UsesConfiguredPrefix()
    {
        const string custom = "CUSTOM_PREFIX_TEST";
        string? previous = Environment.GetEnvironmentVariable(custom);
        try
        {
            Environment.SetEnvironmentVariable(custom, "custom-value");
            var options = new SecretsOptions { EnvironmentVariablePrefix = "CUSTOM_PREFIX_" };
            var provider = new EnvironmentSecretProvider(options);

            string? value = provider.GetSecret("test");

            Assert.Equal("custom-value", value);
        }
        finally
        {
            Environment.SetEnvironmentVariable(custom, previous);
        }
    }

    [Fact]
    public void GetSecret_EmptyName_ReturnsNull()
    {
        var provider = new EnvironmentSecretProvider(new SecretsOptions());

        Assert.Null(provider.GetSecret(string.Empty));
    }
}
