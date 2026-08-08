using LastHour.BuildingBlocks.Infrastructure.Performance;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Configuration;

public class PerformanceBehaviorOptionsValidatorTests
{
    private readonly PerformanceBehaviorOptionsValidator _validator = new PerformanceBehaviorOptionsValidator();

    [Fact]
    public void Validate_ZeroSlowRequestThreshold_Fails()
    {
        var options = new PerformanceBehaviorOptions { SlowRequestThreshold = TimeSpan.Zero };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("SlowRequestThreshold", result.FailureMessage);
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var options = new PerformanceBehaviorOptions { SlowRequestThreshold = TimeSpan.FromSeconds(1) };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
