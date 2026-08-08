using LastHour.Api.Security.Cors;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LastHour.Api.Tests.Validators;

public class CorsOptionsValidatorTests
{
    [Fact]
    public void Validate_AllowAnyOriginInProduction_Fails()
    {
        var validator = new CorsOptionsValidator(new StubEnvironment("Production"));
        var options = new CorsOptions { AllowAnyOrigin = true };

        ValidateOptionsResult result = validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("AllowAnyOrigin", result.FailureMessage);
    }

    [Fact]
    public void Validate_AllowAnyOriginWithCredentials_Fails()
    {
        var validator = new CorsOptionsValidator(new StubEnvironment("Development"));
        var options = new CorsOptions { AllowAnyOrigin = true, AllowCredentials = true };

        ValidateOptionsResult result = validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("AllowCredentials", result.FailureMessage);
    }

    [Fact]
    public void Validate_LiteralStarOrigin_Fails()
    {
        var validator = new CorsOptionsValidator(new StubEnvironment("Development"));
        var options = new CorsOptions { AllowedOrigins = new[] { "*" } };

        ValidateOptionsResult result = validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("AllowAnyOrigin", result.FailureMessage);
    }

    [Fact]
    public void Validate_NonHttpOrigin_Fails()
    {
        var validator = new CorsOptionsValidator(new StubEnvironment("Development"));
        var options = new CorsOptions { AllowedOrigins = new[] { "ftp://example.com" } };

        ValidateOptionsResult result = validator.Validate(null, options);

        Assert.True(result.Failed);
        Assert.Contains("invalid origin", result.FailureMessage);
    }

    [Fact]
    public void Validate_WildcardSubdomainOrigin_Succeeds()
    {
        var validator = new CorsOptionsValidator(new StubEnvironment("Production"));
        var options = new CorsOptions { AllowedOrigins = new[] { "https://*.example.com" } };

        ValidateOptionsResult result = validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public void Validate_DisabledOptions_Succeeds()
    {
        var validator = new CorsOptionsValidator(new StubEnvironment("Production"));
        var options = new CorsOptions { Enabled = false, AllowAnyOrigin = true };

        ValidateOptionsResult result = validator.Validate(null, options);

        Assert.True(result.Succeeded);
    }

    private sealed class StubEnvironment : IHostEnvironment
    {
        public StubEnvironment(string environmentName)
        {
            EnvironmentName = environmentName;
        }

        public string EnvironmentName { get; set; }

        public string ApplicationName { get; set; } = "Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
