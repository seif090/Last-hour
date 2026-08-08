using LastHour.BuildingBlocks.Infrastructure.HealthChecks;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.HealthChecks;

public class HealthChecksOptionsValidatorTests
{
    private readonly HealthChecksOptionsValidator _validator = new();

    [Fact]
    public void Validate_Defaults_ReturnsSuccess()
    {
        var options = new HealthChecksOptions();

        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        Assert.True(result.Succeeded);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Validate_InvalidTimeoutSeconds_Fails(int timeoutSeconds)
    {
        var options = new HealthChecksOptions { TimeoutSeconds = timeoutSeconds };

        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains(nameof(HealthChecksOptions.TimeoutSeconds), StringComparison.Ordinal));
    }

    [Fact]
    public void Validate_NegativeDiskMinimum_Fails()
    {
        var options = new HealthChecksOptions
        {
            Disk = new DiskHealthCheckOptions { MinimumFreeMegabytes = -1 },
        };

        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        Assert.True(result.Failed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_InvalidMemoryMaximum_Fails(long maximumUsedBytes)
    {
        var options = new HealthChecksOptions
        {
            Memory = new MemoryHealthCheckOptions { MaximumUsedBytes = maximumUsedBytes },
        };

        ValidateOptionsResult result = _validator.Validate(Options.DefaultName, options);

        Assert.True(result.Failed);
        Assert.Contains(result.Failures, failure => failure.Contains(nameof(HealthChecksOptions.Memory), StringComparison.Ordinal));
    }
}
