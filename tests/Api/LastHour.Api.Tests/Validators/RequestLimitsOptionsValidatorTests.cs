using LastHour.Api.RequestLimits;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Tests.Validators;

public class RequestLimitsOptionsValidatorTests
{
    private readonly RequestLimitsOptionsValidator _validator = new RequestLimitsOptionsValidator();

    [Fact]
    public void Validate_ZeroMaxRequestBodySize_Fails()
    {
        var options = new RequestLimitsOptions { MaxRequestBodySize = 0 };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("MaxRequestBodySize", result.FailureMessage);
    }

    [Fact]
    public void Validate_NonPositiveKeepAliveTimeout_Fails()
    {
        var options = new RequestLimitsOptions { KeepAliveTimeout = TimeSpan.Zero };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("KeepAliveTimeout", result.FailureMessage);
    }

    [Fact]
    public void Validate_NonPositiveDataRate_Fails()
    {
        var options = new RequestLimitsOptions { MinRequestBodyDataRateBytesPerSecond = 0 };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("MinRequestBodyDataRateBytesPerSecond", result.FailureMessage);
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var options = new RequestLimitsOptions();

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
