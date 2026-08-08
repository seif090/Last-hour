using LastHour.Api.Secrets;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Tests.Validators;

public class SecretsOptionsValidatorTests
{
    private readonly SecretsOptionsValidator _validator = new SecretsOptionsValidator();

    [Fact]
    public void Validate_EmptyPrefixWhenEnabled_Fails()
    {
        var options = new SecretsOptions { EnvironmentVariablePrefix = string.Empty };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("EnvironmentVariablePrefix", result.FailureMessage);
    }

    [Fact]
    public void Validate_EmptySecretName_Fails()
    {
        var options = new SecretsOptions { Names = new[] { string.Empty } };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("Names", result.FailureMessage);
    }

    [Fact]
    public void Validate_DisabledOptions_Succeeds()
    {
        var options = new SecretsOptions { Enabled = false, EnvironmentVariablePrefix = string.Empty };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var options = new SecretsOptions();

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
