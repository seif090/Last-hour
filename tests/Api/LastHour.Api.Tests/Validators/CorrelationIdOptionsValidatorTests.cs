using LastHour.Api.Middleware;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Tests.Validators;

public class CorrelationIdOptionsValidatorTests
{
    private readonly CorrelationIdOptionsValidator _validator = new CorrelationIdOptionsValidator();

    [Fact]
    public void Validate_EmptyHeaderName_Fails()
    {
        var options = new CorrelationIdOptions { HeaderName = string.Empty };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("HeaderName", result.FailureMessage);
    }

    [Fact]
    public void Validate_NonPositiveMaximumIncomingLength_Fails()
    {
        var options = new CorrelationIdOptions { MaximumIncomingLength = 0 };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("MaximumIncomingLength", result.FailureMessage);
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var options = new CorrelationIdOptions();

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
