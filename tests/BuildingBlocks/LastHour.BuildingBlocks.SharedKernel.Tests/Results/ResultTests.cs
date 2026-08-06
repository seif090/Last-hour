using LastHour.BuildingBlocks.SharedKernel.Results;

namespace LastHour.BuildingBlocks.SharedKernel.Tests.Results;

public class ResultTests
{
    [Fact]
    public void Success_IsSuccess_WithoutErrors()
    {
        Result result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Empty(result.Errors);
        Assert.Null(result.FirstError);
        Assert.Equal(ErrorType.None, result.ErrorType);
    }

    [Fact]
    public void Failure_IsFailure_WithError()
    {
        Error error = Error.NotFound("NF", "Missing");
        Result result = Result.Failure(error);

        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.FirstError);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Single(result.Errors);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        Result result = Error.Conflict("CONFLICT", "Conflict");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ExposesAllErrors()
    {
        Error first = Error.Failure("E1", "First");
        Error second = Error.Conflict("E2", "Second");
        Result result = Result.Failure(new[] { first, second });

        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(first, result.FirstError);
    }

    [Fact]
    public void Failure_WithEmptyErrors_Throws()
    {
        Assert.Throws<ArgumentException>(() => Result.Failure(Array.Empty<Error>()));
    }

    [Fact]
    public void Ensure_WhenPredicateIsTrue_KeepsSuccess()
    {
        Result result = Result.Success().Ensure(() => true, Error.Failure("F", "fail"));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Ensure_WhenPredicateIsFalse_ReturnsFailure()
    {
        Error error = Error.Failure("F", "fail");
        Result result = Result.Success().Ensure(() => false, error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public void Ensure_OnAlreadyFailedResult_DoesNotInvokePredicate()
    {
        bool invoked = false;
        Result result = Result.Failure(Error.Failure("F", "fail")).Ensure(() =>
        {
            invoked = true;
            return false;
        }, Error.Failure("F2", "fail"));

        Assert.True(result.IsFailure);
        Assert.False(invoked);
    }

    [Fact]
    public void Map_OnSuccess_ProducesValue()
    {
        Result<int> result = Result.Success().Map(() => 42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesErrors()
    {
        Error first = Error.Failure("E1", "First");
        Error second = Error.Conflict("E2", "Second");
        Result<int> result = Result.Failure(new[] { first, second }).Map(() => 42);

        Assert.True(result.IsFailure);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(first, result.FirstError);
    }

    [Fact]
    public void Bind_OnSuccess_ReturnsChainedResult()
    {
        Result<int> result = Result.Success().Bind(() => Result<int>.Success(7));

        Assert.True(result.IsSuccess);
        Assert.Equal(7, result.Value);
    }

    [Fact]
    public void Bind_OnFailure_ShortCircuits()
    {
        Error error = Error.Failure("F", "fail");
        Result<int> result = Result.Failure(error).Bind(() => Result<int>.Success(7));

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public void Tap_InvokesActionOnlyOnSuccess()
    {
        int invocations = 0;

        Result.Success().Tap(() => invocations++);
        Result.Failure(Error.Failure("F", "fail")).Tap(() => invocations++);

        Assert.Equal(1, invocations);
    }

    [Fact]
    public void Match_ReturnsOnSuccessValue()
    {
        int value = Result.Success().Match(() => 1, _ => 0);

        Assert.Equal(1, value);
    }

    [Fact]
    public void Match_ReturnsOnFailureValue()
    {
        Error error = Error.NotFound("NF", "Missing");
        int value = Result.Failure(error).Match(() => 1, e => e.Type == ErrorType.NotFound ? 2 : 0);

        Assert.Equal(2, value);
    }
}
