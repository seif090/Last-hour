using System.Reflection;

namespace LastHour.Api.Observability.Logging;

/// <summary>
/// Resolves the application version used to enrich log events, taken from the entry assembly's
/// informational version (the "1.2.3+commithash" form produced by source-link) and falling back
/// to the plain assembly version when the informational version is not available.
/// </summary>
public static class ApplicationVersion
{
    /// <summary>
    /// Gets the version of the application, or "unknown" when it cannot be resolved.
    /// </summary>
    /// <returns>The informational version of the entry assembly.</returns>
    public static string Get()
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? typeof(ApplicationVersion).Assembly;

        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? assembly.GetName().Version?.ToString()
            ?? "unknown";
    }
}
