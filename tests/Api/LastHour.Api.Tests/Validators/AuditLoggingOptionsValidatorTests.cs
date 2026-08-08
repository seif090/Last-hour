using LastHour.Api.Observability.Auditing;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Tests.Validators;

public class AuditLoggingOptionsValidatorTests
{
    private readonly AuditLoggingOptionsValidator _validator = new AuditLoggingOptionsValidator();

    [Fact]
    public void Validate_EmptyFilePath_Fails()
    {
        var options = new AuditLoggingOptions { FilePath = string.Empty };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("FilePath", result.FailureMessage);
    }

    [Fact]
    public void Validate_ZeroRetainedFileCount_Fails()
    {
        var options = new AuditLoggingOptions { RetainedFileCount = 0 };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("RetainedFileCount", result.FailureMessage);
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var options = new AuditLoggingOptions();

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
