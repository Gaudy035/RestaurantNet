namespace backend.Services.Errors;

public sealed class Error
{
    public ErrorCode Code { get; }
    public string Message { get; }
    public int StatusCode { get; }

    private Error (ErrorCode code, string message, int statusCode)
    {
        Code = code;
        Message = message;
        StatusCode = statusCode;
    }

    public static Error From(ErrorCode code) => 
        code switch
        {  
            ErrorCode.InvalidRefreshToken => new Error(code, "Invalid refresh token", StatusCodes.Status401Unauthorized),
            ErrorCode.InvalidCredentials => new Error(code, "Invalid credentials", StatusCodes.Status401Unauthorized),
            ErrorCode.EmailAlreadyTaken => new Error(code, "Email already taken", StatusCodes.Status409Conflict),
            ErrorCode.AlreadyAssigned => new Error(code, "Employee already assigned", StatusCodes.Status409Conflict),
            ErrorCode.OwnAccountDeletion => new Error(code, "Cannot delete own account", StatusCodes.Status400BadRequest),
            ErrorCode.DbOperationFailed => new Error(code, "Database operation failed", StatusCodes.Status500InternalServerError),
            ErrorCode.ClientNotFound => new Error(code, "Client not found", StatusCodes.Status404NotFound),
            ErrorCode.EmployeeNotFound => new Error(code, "Employee not found", StatusCodes.Status404NotFound),
            ErrorCode.LocationNotFound => new Error(code, "Location not found", StatusCodes.Status404NotFound),
            ErrorCode.AssignmentNotFound => new Error(code, "Assignment not found", StatusCodes.Status404NotFound),
            _ => new Error(code, "Unknown error", StatusCodes.Status500InternalServerError)
        };
}