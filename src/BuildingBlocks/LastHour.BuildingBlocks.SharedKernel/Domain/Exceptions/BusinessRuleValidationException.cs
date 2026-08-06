namespace LastHour.BuildingBlocks.SharedKernel.Domain.Exceptions;

/// <summary>
/// Represents an exception thrown when a business rule is violated during domain validation.
/// </summary>
public class BusinessRuleValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleValidationException"/> class.
    /// </summary>
    /// <param name="rule">The business rule that was violated.</param>
    /// <exception cref="ArgumentNullException"><paramref name="rule"/> is <see langword="null"/>.</exception>
    public BusinessRuleValidationException(IBusinessRule rule)
        : base(GetMessage(rule))
    {
        ArgumentNullException.ThrowIfNull(rule);
        Rule = rule;
    }

    /// <summary>
    /// Gets the business rule that was violated.
    /// </summary>
    public IBusinessRule Rule { get; }

    /// <inheritdoc/>
    public override string ToString() => $"{Rule.GetType().Name}: {Message}";

    private static string GetMessage(IBusinessRule rule)
        => rule?.Message ?? throw new ArgumentNullException(nameof(rule));
}
