using LastHour.Api.Observability.Telemetry;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Tests.Validators;

public class OpenTelemetryOptionsValidatorTests
{
    private readonly OpenTelemetryOptionsValidator _validator = new OpenTelemetryOptionsValidator();

    [Fact]
    public void Validate_EmptyServiceName_Fails()
    {
        var options = new OpenTelemetryOptions { ServiceName = string.Empty };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("ServiceName", result.FailureMessage);
    }

    [Fact]
    public void Validate_OtlpExporterWithoutEndpoint_Fails()
    {
        var options = new OpenTelemetryOptions { UseOtlpExporter = true, OtlpEndpoint = null };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("OtlpEndpoint", result.FailureMessage);
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var options = new OpenTelemetryOptions { UseOtlpExporter = true, OtlpEndpoint = "http://localhost:4317" };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
