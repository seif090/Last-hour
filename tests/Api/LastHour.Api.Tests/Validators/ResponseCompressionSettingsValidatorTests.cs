using System.IO.Compression;
using LastHour.Api.Compression;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Tests.Validators;

public class ResponseCompressionSettingsValidatorTests
{
    private readonly ResponseCompressionSettingsValidator _validator = new ResponseCompressionSettingsValidator();

    [Fact]
    public void Validate_UndefinedCompressionLevel_Fails()
    {
        var options = new ResponseCompressionSettings { CompressionLevel = (CompressionLevel)99 };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("CompressionLevel", result.FailureMessage);
    }

    [Fact]
    public void Validate_ValidOptions_Succeeds()
    {
        var options = new ResponseCompressionSettings { CompressionLevel = CompressionLevel.SmallestSize };

        ValidateOptionsResult result = _validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }
}
