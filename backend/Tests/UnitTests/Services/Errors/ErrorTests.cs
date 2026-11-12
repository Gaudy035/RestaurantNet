using backend.Services.Errors;

namespace UnitTests.Services.Errors;

public class ErrorTests
{
    [Theory]
    [InlineData(ErrorCode.InvalidRefreshToken, 401)]
    [InlineData(ErrorCode.InvalidCredentials, 401)]
    [InlineData(ErrorCode.EmailAlreadyTaken, 409)]
    [InlineData(ErrorCode.AlreadyAssigned, 409)]
    [InlineData(ErrorCode.OwnAccountDeletion, 400)]
    [InlineData(ErrorCode.DbOperationFailed, 500)]
    [InlineData(ErrorCode.ClientNotFound, 404)]
    [InlineData(ErrorCode.EmployeeNotFound, 404)]
    [InlineData(ErrorCode.LocationNotFound, 404)]
    [InlineData(ErrorCode.LocationAlreadyExists, 409)]
    [InlineData(ErrorCode.AssignmentNotFound, 404)]
    [InlineData(ErrorCode.CategoryNotFound, 404)]
    [InlineData(ErrorCode.CategoryAlreadyExists, 409)]
    [InlineData(ErrorCode.CategoryContainsItems, 409)]
    [InlineData(ErrorCode.MenuItemNotFound, 404)]
    [InlineData(ErrorCode.UpdateBodyEmpty, 400)]
    [InlineData(ErrorCode.CartItemNotFound, 404)]
    public void From_MapsErrorCodeToExpectedStatusCode(ErrorCode code, int expectedStatusCode)
    {
        var error = Error.From(code);

        Assert.Equal(code, error.Code);
        Assert.Equal(expectedStatusCode, error.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
    }

    [Fact]
    public void Result_Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ResultOfT_Fail_ExposesErrorAndNoData()
    {
        var result = Result<string>.Fail(ErrorCode.ClientNotFound);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.Equal(ErrorCode.ClientNotFound, result.Error!.Code);
    }
}
