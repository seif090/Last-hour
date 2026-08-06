namespace LastHour.Api.ProblemDetails;

/// <summary>
/// Endpoint filter that inspects the value an endpoint returned and converts failed
/// SharedKernel results into RFC 7807 <see cref="Microsoft.AspNetCore.Mvc.ProblemDetails"/>
/// responses. Endpoints that return a <see cref="LastHour.BuildingBlocks.SharedKernel.Results.Result"/>
/// or <see cref="LastHour.BuildingBlocks.SharedKernel.Results.Result{TValue}"/> therefore get a
/// consistent error contract without mapping the failure themselves.
/// </summary>
public sealed class ResultEndpointFilter : IEndpointFilter
{
    /// <summary>
    /// Converts a failed result returned by the endpoint into a problem details response.
    /// </summary>
    /// <param name="context">The endpoint filter invocation context.</param>
    /// <param name="next">The next filter in the chain, which invokes the endpoint.</param>
    /// <returns>The mapped response object, or the endpoint result when it is not a result.</returns>
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        object? result = await next(context);

        return ResultProblemDetailsMapper.TryMap(result, out object? mapped) ? mapped : result;
    }
}
