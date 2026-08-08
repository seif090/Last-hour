using LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Configuration;

public class OutboxOptionsValidatorTests
{
    private readonly OutboxOptionsValidator _validator = new OutboxOptionsValidator();

    [Fact]
    public void Validate_SubSecondProcessingInterval_Fails()
    {
        var options = new OutboxOptions { ProcessingInterval = TimeSpan.FromMilliseconds(500) };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("ProcessingInterval", result.FailureMessage);
    }

    [Fact]
    public void Validate_ZeroBatchSize_Fails()
    {
        var options = new OutboxOptions { BatchSize = 0 };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("BatchSize", result.FailureMessage);
    }

    [Fact]
    public void Validate_ZeroMaxRetryCount_Fails()
    {
        var options = new OutboxOptions { MaxRetryCount = 0 };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("MaxRetryCount", result.FailureMessage);
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var options = new OutboxOptions();

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
