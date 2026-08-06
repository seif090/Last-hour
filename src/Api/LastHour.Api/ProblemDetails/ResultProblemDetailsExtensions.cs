namespace LastHour.Api.ProblemDetails;

/// <summary>
/// Extensions that attach the result-to-problem-details mapping to endpoint routes.
/// </summary>
public static class ResultProblemDetailsExtensions
{
    /// <summary>
    /// Adds the <see cref="ResultEndpointFilter"/> so a <see cref="LastHour.BuildingBlocks.SharedKernel.Results.Result"/>
    /// returned by the endpoint is automatically converted into an RFC 7807 problem details response on failure.
    /// </summary>
    /// <param name="builder">The route handler builder to extend.</param>
    /// <returns>The same route handler builder, so calls can be chained.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="builder"/> is <see langword="null"/>.</exception>
    public static RouteHandlerBuilder MapResultToProblemDetails(this RouteHandlerBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.AddEndpointFilter<ResultEndpointFilter>();
    }
}
