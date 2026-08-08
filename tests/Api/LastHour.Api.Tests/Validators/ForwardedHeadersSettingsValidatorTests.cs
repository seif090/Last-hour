using LastHour.Api.Security.ForwardedHeaders;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Tests.Validators;

public class ForwardedHeadersSettingsValidatorTests
{
    private readonly ForwardedHeadersSettingsValidator _validator = new ForwardedHeadersSettingsValidator();

    [Fact]
    public void Validate_ForwardLimitAboveOneWithoutKnownHosts_Fails()
    {
        var options = new ForwardedHeadersSettings { ForwardLimit = 2 };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("KnownProxies", result.FailureMessage);
    }

    [Fact]
    public void Validate_ForwardLimitAboveOneWithKnownProxy_Succeeds()
    {
        var options = new ForwardedHeadersSettings { ForwardLimit = 2, KnownProxies = new[] { "10.0.0.1" } };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_InvalidKnownProxy_Fails()
    {
        var options = new ForwardedHeadersSettings { KnownProxies = new[] { "not-an-ip" } };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("invalid IP address", result.FailureMessage);
    }

    [Fact]
    public void Validate_InvalidKnownNetwork_Fails()
    {
        var options = new ForwardedHeadersSettings { KnownNetworks = new[] { "10.0.0.0/not-a-prefix" } };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("invalid CIDR network", result.FailureMessage);
    }

    [Fact]
    public void Validate_ValidSettings_Succeeds()
    {
        var options = new ForwardedHeadersSettings();

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
