using LastHour.Api.ProblemDetails;
using LastHour.BuildingBlocks.SharedKernel.Results;
using Microsoft.AspNetCore.Http;

namespace LastHour.Api.Tests.ProblemDetails;

/// <summary>
/// Exercises the mapping from SharedKernel errors and results onto RFC 7807 problem details.
/// </summary>
public class ErrorToProblemDetailsMapperTests
{
    [Fact]
    public void Map_ValidationError_SetsBadRequestStatusAndValidationErrors()
    {
        var error = new ValidationError(
            "ValidationFailed",
            "One or more validation errors occurred.",
            new[]
            {
                Error.Validation("Email", "'Email' is required."),
                Error.Validation("Password", "'Password' must be at least 8 characters."),
            });

        var problemDetails = ErrorToProblemDetailsMapper.Map(error);

        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Equal("Bad Request", problemDetails.Title);
        Assert.Equal("https://lasthour.dev/errors/validation", problemDetails.Type);
        Assert.Equal("One or more validation errors occurred.", problemDetails.Detail);
        Assert.Equal("ValidationFailed", problemDetails.Extensions["code"]);

        var errors = Assert.IsType<Dictionary<string, string[]>>(problemDetails.Extensions["errors"]);
        Assert.Equal("'Email' is required.", Assert.Single(errors["Email"]));
        Assert.Equal("'Password' must be at least 8 characters.", Assert.Single(errors["Password"]));
    }

    [Fact]
    public void Map_NotFoundError_SetsNotFoundStatus()
    {
        var problemDetails = ErrorToProblemDetailsMapper.Map(Error.NotFound("ResourceNotFound", "Missing."));

        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
        Assert.Equal("Not Found", problemDetails.Title);
        Assert.Equal("https://lasthour.dev/errors/notfound", problemDetails.Type);
        Assert.Equal("ResourceNotFound", problemDetails.Extensions["code"]);
        Assert.False(problemDetails.Extensions.ContainsKey("errors"));
    }

    [Fact]
    public void Map_ConflictError_SetsConflictStatus()
    {
        var problemDetails = ErrorToProblemDetailsMapper.Map(Error.Conflict("ResourceConflict", "Conflicts."));

        Assert.Equal(StatusCodes.Status409Conflict, problemDetails.Status);
        Assert.Equal("Conflict", problemDetails.Title);
        Assert.Equal("https://lasthour.dev/errors/conflict", problemDetails.Type);
        Assert.Equal("ResourceConflict", problemDetails.Extensions["code"]);
    }

    [Fact]
    public void Map_UnauthorizedError_SetsUnauthorizedStatus()
    {
        var problemDetails = ErrorToProblemDetailsMapper.Map(Error.Unauthorized("Unauthorized", "Not authenticated."));

        Assert.Equal(StatusCodes.Status401Unauthorized, problemDetails.Status);
        Assert.Equal("Unauthorized", problemDetails.Title);
        Assert.Equal("https://lasthour.dev/errors/unauthorized", problemDetails.Type);
        Assert.Equal("Unauthorized", problemDetails.Extensions["code"]);
    }

    [Fact]
    public void Map_ForbiddenError_SetsForbiddenStatus()
    {
        var problemDetails = ErrorToProblemDetailsMapper.Map(Error.Forbidden("Forbidden", "Not allowed."));

        Assert.Equal(StatusCodes.Status403Forbidden, problemDetails.Status);
        Assert.Equal("Forbidden", problemDetails.Title);
        Assert.Equal("https://lasthour.dev/errors/forbidden", problemDetails.Type);
        Assert.Equal("Forbidden", problemDetails.Extensions["code"]);
    }

    [Fact]
    public void Map_FailureError_SetsInternalServerErrorStatus()
    {
        var problemDetails = ErrorToProblemDetailsMapper.Map(Error.Failure("UnhandledException", "Boom."));

        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("Internal Server Error", problemDetails.Title);
        Assert.Equal("https://lasthour.dev/errors/failure", problemDetails.Type);
        Assert.Equal("UnhandledException", problemDetails.Extensions["code"]);
    }

    [Fact]
    public void TryMap_FailedResult_ReturnsProblemResponse()
    {
        Result result = Result.Failure(Error.NotFound("ResourceNotFound", "Missing."));

        bool mapped = ResultProblemDetailsMapper.TryMap(result, out object? response);

        Assert.True(mapped);
        Assert.IsAssignableFrom<IResult>(response);
        var resultResponse = Assert.IsAssignableFrom<Microsoft.AspNetCore.Http.HttpResults.ProblemHttpResult>(response);
        Assert.Equal(StatusCodes.Status404NotFound, resultResponse.StatusCode);
    }

    [Fact]
    public void TryMap_SuccessfulResultOfT_UnwrapsValue()
    {
        Result<int> result = Result<int>.Success(42);

        bool mapped = ResultProblemDetailsMapper.TryMap(result, out object? response);

        Assert.True(mapped);
        Assert.Equal(42, response);
    }

    [Fact]
    public void TryMap_SuccessfulResult_ReturnsOkResponse()
    {
        Result result = Result.Success();

        bool mapped = ResultProblemDetailsMapper.TryMap(result, out object? response);

        Assert.True(mapped);
        Assert.IsAssignableFrom<IResult>(response);
    }

    [Fact]
    public void TryMap_NonResultValue_ReturnsUnmapped()
    {
        bool mapped = ResultProblemDetailsMapper.TryMap("plain value", out object? response);

        Assert.False(mapped);
        Assert.Null(response);
    }
}
