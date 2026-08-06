using System.Globalization;
using LastHour.BuildingBlocks.SharedKernel.Results;

namespace LastHour.BuildingBlocks.SharedKernel.Tests.Results;

public class ResultOfTTests
{
    [Fact]
    public void Success_ExposesValue()
    {
        Result<int> result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
        Assert.Equal(42, result.ValueOrDefault);
    }

    [Fact]
    public void Failure_ValueAccess_Throws()
    {
        Result<int> result = Result<int>.Failure(Error.Failure("F", "fail"));

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }

    [Fact]
    public void Failure_ValueOrDefault_IsDefault()
    {
        Result<int> result = Result<int>.Failure(Error.Failure("F", "fail"));

        Assert.Equal(0, result.ValueOrDefault);
    }

    [Fact]
    public void Failure_ValueOrDefault_ForReferenceType_IsNull()
    {
        Result<string> result = Result<string>.Failure(Error.Failure("F", "fail"));

        Assert.Null(result.ValueOrDefault);
    }

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        Result<string> result = "hello";

        Assert.True(result.IsSuccess);
        Assert.Equal("hello", result.Value);
    }

    [Fact]
    public void ImplicitConversion_FromError_CreatesFailure()
    {
        Result<string> result = Error.NotFound("NF", "Missing");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
    }

    [Fact]
    public void Failure_WithMultipleErrors_ExposesAllErrors()
    {
        Error first = Error.Failure("E1", "First");
        Error second = Error.Validation("E2", "Second");
        Result<int> result = Result<int>.Failure(new[] { first, second });

        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(first, result.FirstError);
        Assert.Equal(ErrorType.Failure, result.ErrorType);
    }

    [Fact]
    public void TryGetValue_OnSuccess_ReturnsTrueAndValue()
    {
        Result<int> result = Result<int>.Success(5);

        Assert.True(result.TryGetValue(out int value));
        Assert.Equal(5, value);
    }

    [Fact]
    public void TryGetValue_OnFailure_ReturnsFalseAndDefault()
    {
        Result<int> result = Result<int>.Failure(Error.Failure("F", "fail"));

        Assert.False(result.TryGetValue(out int value));
        Assert.Equal(0, value);
    }

    [Fact]
    public void Map_TransformsValue()
    {
        Result<int> result = Result<int>.Success(3).Map(value => value * 2);

        Assert.True(result.IsSuccess);
        Assert.Equal(6, result.Value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesErrors()
    {
        Error error = Error.Failure("F", "fail");
        Result<string> result = Result<int>.Failure(error).Map(value => value.ToString(CultureInfo.InvariantCulture));

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public void Bind_ChainsOnSuccess()
    {
        Result<int> result = Result<int>.Success(3).Bind(value => Result<int>.Success(value * 2));

        Assert.True(result.IsSuccess);
        Assert.Equal(6, result.Value);
    }

    [Fact]
    public void Bind_ShortCircuitsOnFailure()
    {
        Error error = Error.Conflict("CONFLICT", "Conflict");
        Result<int> result = Result<int>.Failure(error).Bind(value => Result<int>.Success(value * 2));

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public void Ensure_WhenPredicateIsSatisfied_KeepsValue()
    {
        Result<int> result = Result<int>.Success(5).Ensure(value => value > 0, Error.Failure("INVALID", "Invalid"));

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Value);
    }

    [Fact]
    public void Ensure_WhenPredicateIsViolated_ReturnsFailure()
    {
        Error error = Error.Failure("INVALID", "Invalid");
        Result<int> result = Result<int>.Success(-5).Ensure(value => value > 0, error);

        Assert.True(result.IsFailure);
        Assert.Equal(error, result.FirstError);
    }

    [Fact]
    public void Tap_InvokesActionOnlyOnSuccess()
    {
        int invocations = 0;

        Result<int>.Success(1).Tap(_ => invocations++);
        Result<int>.Failure(Error.Failure("F", "fail")).Tap(_ => invocations++);

        Assert.Equal(1, invocations);
    }

    [Fact]
    public void Match_ReturnsOnSuccessValue()
    {
        int value = Result<int>.Success(4).Match<int>(x => x * 10, _ => 0);

        Assert.Equal(40, value);
    }

    [Fact]
    public void Match_ReturnsOnFailureValue()
    {
        Error error = Error.NotFound("NF", "Missing");
        int value = Result<int>.Failure(error).Match<int>(_ => 0, e => e.Type == ErrorType.NotFound ? 2 : 0);

        Assert.Equal(2, value);
    }
}
