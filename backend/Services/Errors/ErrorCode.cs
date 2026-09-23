namespace backend.Services.Errors;

public enum ErrorCode
{
    InvalidRefreshToken,
    InvalidCredentials,
    EmailAlreadyTaken,
    NotFound,
    AlreadyAssigned,
    OwnAccountDeletion,
    DbOperationFailed,
    ClientNotFound,
    LocationNotFound,
    EmployeeNotFound,
    AssignmentNotFound
}