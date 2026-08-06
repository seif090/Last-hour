using LastHour.BuildingBlocks.SharedKernel.Results;

namespace LastHour.BuildingBlocks.SharedKernel.Tests.Results;

public class ErrorTests
{
    [Fact]
    public void Failure_CreatesFailureTypeError()
    {
        Error error = Error.Failure("ERR_CODE", "Something went wrong");

        Assert.Equal(ErrorType.Failure, error.Type);
        Assert.Equal("ERR_CODE", error.Code);
        Assert.Equal("Something went wrong", error.Description);
    }

    [Fact]
    public void Validation_CreatesValidationError()
    {
        Error error = Error.Validation("INVALID", "Input is invalid");

        Assert.Equal(ErrorType.Validation, error.Type);
        Assert.IsType<ValidationError>(error);
    }

    [Fact]
    public void NotFound_CreatesNotFoundError()
    {
        Error error = Error.NotFound("NOT_FOUND", "Resource was not found");

        Assert.Equal(ErrorType.NotFound, error.Type);
        Assert.IsType<NotFoundError>(error);
    }

    [Fact]
    public void Conflict_CreatesConflictError()
    {
        Error error = Error.Conflict("CONFLICT", "State conflict");

        Assert.Equal(ErrorType.Conflict, error.Type);
        Assert.IsType<ConflictError>(error);
    }

    [Fact]
    public void Unauthorized_CreatesUnauthorizedError()
    {
        Error error = Error.Unauthorized("UNAUTHORIZED", "Authentication required");

        Assert.Equal(ErrorType.Unauthorized, error.Type);
        Assert.IsType<UnauthorizedError>(error);
    }

    [Fact]
    public void Forbidden_CreatesForbiddenError()
    {
        Error error = Error.Forbidden("FORBIDDEN", "Access denied");

        Assert.Equal(ErrorType.Forbidden, error.Type);
        Assert.IsType<ForbiddenError>(error);
    }

    [Fact]
    public void SpecificErrorTypes_ExposeExpectedErrorType()
    {
        Assert.Equal(ErrorType.NotFound, new NotFoundError("C", "d").Type);
        Assert.Equal(ErrorType.Conflict, new ConflictError("C", "d").Type);
        Assert.Equal(ErrorType.Unauthorized, new UnauthorizedError("C", "d").Type);
        Assert.Equal(ErrorType.Forbidden, new ForbiddenError("C", "d").Type);
        Assert.Equal(ErrorType.Validation, new ValidationError("C", "d").Type);
    }

    [Fact]
    public void Error_Equality_IsValueBased()
    {
        Error left = Error.NotFound("NF", "Missing");
        Error right = Error.NotFound("NF", "Missing");

        Assert.Equal(left, right);
        Assert.Equal(left.GetHashCode(), right.GetHashCode());
    }

    [Fact]
    public void Error_Inequality_DistinguishesCodeAndType()
    {
        Assert.NotEqual(Error.NotFound("NF", "Missing"), Error.NotFound("NF2", "Missing"));
        Assert.NotEqual(Error.NotFound("NF", "Missing"), Error.Conflict("NF", "Missing"));
    }

    [Fact]
    public void ValidationError_CarriesIndividualValidations()
    {
        Error inner = Error.Validation("FIELD_INVALID", "Field is invalid");
        ValidationError error = new ValidationError("VALIDATION", "Validation failed", new[] { inner });

        Assert.Single(error.Validations);
        Assert.Contains(inner, error.Validations);
    }

    [Fact]
    public void ValidationError_WithoutValidations_ExposesEmptyList()
    {
        ValidationError error = new ValidationError("VALIDATION", "Validation failed");

        Assert.Empty(error.Validations);
    }

    [Fact]
    public void ToString_ContainsCodeAndDescription()
    {
        Error error = Error.Conflict("CONFLICT", "Already exists");

        Assert.Contains("CONFLICT", error.ToString());
        Assert.Contains("Already exists", error.ToString());
    }
}
