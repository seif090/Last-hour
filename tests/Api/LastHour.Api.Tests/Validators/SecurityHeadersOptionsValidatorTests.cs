using LastHour.Api.Security.SecurityHeaders;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Tests.Validators;

public class SecurityHeadersOptionsValidatorTests
{
    private readonly SecurityHeadersOptionsValidator _validator = new SecurityHeadersOptionsValidator();

    [Fact]
    public void Validate_ZeroHstsMaxAge_Fails()
    {
        var options = new SecurityHeadersOptions();
        options.Hsts.MaxAgeDays = 0;

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("MaxAgeDays", result.FailureMessage);
    }

    [Fact]
    public void Validate_EmptyContentSecurityPolicy_Fails()
    {
        var options = new SecurityHeadersOptions { ContentSecurityPolicy = string.Empty };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("ContentSecurityPolicy", result.FailureMessage);
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var options = new SecurityHeadersOptions();

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
