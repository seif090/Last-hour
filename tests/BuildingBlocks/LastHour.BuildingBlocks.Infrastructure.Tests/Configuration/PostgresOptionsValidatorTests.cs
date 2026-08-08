using LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;
using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Tests.Configuration;

public class PostgresOptionsValidatorTests
{
    private readonly PostgresOptionsValidator _validator = new PostgresOptionsValidator();

    [Fact]
    public void Validate_MissingConnectionString_Fails()
    {
        var options = new PostgresOptions { ConnectionString = string.Empty };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("ConnectionString", result.FailureMessage);
    }

    [Fact]
    public void Validate_NegativeMaxRetryCount_Fails()
    {
        var options = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            MaxRetryCount = -1,
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("MaxRetryCount", result.FailureMessage);
    }

    [Fact]
    public void Validate_ZeroMaxRetryDelay_Fails()
    {
        var options = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            MaxRetryDelay = TimeSpan.Zero,
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("MaxRetryDelay", result.FailureMessage);
    }

    [Fact]
    public void Validate_NegativeCommandTimeout_Fails()
    {
        var options = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            CommandTimeoutSeconds = -1,
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("CommandTimeoutSeconds", result.FailureMessage);
    }

    [Fact]
    public void Validate_NegativeMaxPoolSize_Fails()
    {
        var options = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            MaxPoolSize = -1,
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("MaxPoolSize", result.FailureMessage);
    }

    [Fact]
    public void Validate_NegativeMinPoolSize_Fails()
    {
        var options = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            MinPoolSize = -1,
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("MinPoolSize", result.FailureMessage);
    }

    [Fact]
    public void Validate_ZeroConnectionIdleLifetime_Fails()
    {
        var options = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            ConnectionIdleLifetime = TimeSpan.Zero,
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("ConnectionIdleLifetime", result.FailureMessage);
    }

    [Fact]
    public void Validate_ZeroConnectionPruningInterval_Fails()
    {
        var options = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            ConnectionPruningInterval = TimeSpan.Zero,
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("ConnectionPruningInterval", result.FailureMessage);
    }

    [Fact]
    public void Validate_NegativeConnectionTimeout_Fails()
    {
        var options = new PostgresOptions
        {
            ConnectionString = "Host=localhost;Database=test",
            ConnectionTimeout = TimeSpan.FromSeconds(-1),
        };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("ConnectionTimeout", result.FailureMessage);
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var options = new PostgresOptions { ConnectionString = "Host=localhost;Database=test" };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
