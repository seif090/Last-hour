using Microsoft.Extensions.Options;

namespace LastHour.BuildingBlocks.Infrastructure.Persistence.Configuration;

/// <summary>
/// Validates <see cref="PostgresOptions"/> so misconfiguration fails fast at startup instead
/// of surfacing as runtime database failures.
/// </summary>
public sealed class PostgresOptionsValidator : IValidateOptions<PostgresOptions>
{
    /// <inheritdoc/>
    public ValidateOptionsResult Validate(string? name, PostgresOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail(
                $"'{PostgresOptions.SectionName}:ConnectionString' is required. Configure it under " +
                $"the '{PostgresOptions.SectionName}' section or the standard 'ConnectionStrings:Postgres' entry.");
        }

        if (options.MaxRetryCount < 0)
        {
            return ValidateOptionsResult.Fail(
                $"'{PostgresOptions.SectionName}:MaxRetryCount' must be greater than or equal to zero.");
        }

        if (options.MaxRetryDelay <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"'{PostgresOptions.SectionName}:MaxRetryDelay' must be a positive duration.");
        }

        if (options.CommandTimeoutSeconds < 0)
        {
            return ValidateOptionsResult.Fail(
                $"'{PostgresOptions.SectionName}:CommandTimeoutSeconds' must be greater than or equal to zero.");
        }

        if (options.MaxPoolSize < 0)
        {
            return ValidateOptionsResult.Fail(
                $"'{PostgresOptions.SectionName}:MaxPoolSize' must be greater than or equal to zero.");
        }

        if (options.MinPoolSize < 0)
        {
            return ValidateOptionsResult.Fail(
                $"'{PostgresOptions.SectionName}:MinPoolSize' must be greater than or equal to zero.");
        }

        if (options.ConnectionIdleLifetime <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"'{PostgresOptions.SectionName}:ConnectionIdleLifetime' must be a positive duration.");
        }

        if (options.ConnectionPruningInterval <= TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"'{PostgresOptions.SectionName}:ConnectionPruningInterval' must be a positive duration.");
        }

        if (options.ConnectionTimeout < TimeSpan.Zero)
        {
            return ValidateOptionsResult.Fail(
                $"'{PostgresOptions.SectionName}:ConnectionTimeout' must be a non-negative duration.");
        }

        return ValidateOptionsResult.Success;
    }
}
