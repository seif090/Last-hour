using System.Text.RegularExpressions;

namespace LastHour.Api.ProblemDetails;

/// <summary>
/// Redacts secrets that commonly appear in exception messages, such as connection strings and
/// credential pairs, so they never reach an HTTP response.
/// </summary>
public static class ConnectionStringRedactor
{
    private static readonly Regex ConnectionStringReference = new (
        @"\bconnection\s*string\s*[:=][^\r\n]*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SecretPair = new (
        @"\b(password|pwd|user\s?id|user\s?name|access\s?token|api\s?key|secret)\s*=\s*[^;\r\n]*",
        RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Returns the text with connection strings and credential pairs replaced by a placeholder.
    /// </summary>
    /// <param name="text">The text to sanitize.</param>
    /// <returns>The sanitized text, or <see cref="string.Empty"/> when the input is null or blank.</returns>
    public static string Redact(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string withoutConnectionString = ConnectionStringReference.Replace(text, "connection string: ***");
        return SecretPair.Replace(withoutConnectionString, "$1=***");
    }
}
