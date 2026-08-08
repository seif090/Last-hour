namespace LastHour.Api.Secrets;

/// <summary>
/// Provides access to secret values that must not live in configuration files or source
/// control. Implementations resolve a named secret from a secure source such as environment
/// variables, user secrets or a key vault.
/// </summary>
public interface ISecretProvider
{
    /// <summary>
    /// Resolves the value of the secret identified by <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The logical, application-facing secret name, for example
    /// <c>ConnectionStrings:Postgres</c>.</param>
    /// <returns>The secret value, or <see langword="null"/> when the secret is not available
    /// from the backing source.</returns>
    string? GetSecret(string name);
}
