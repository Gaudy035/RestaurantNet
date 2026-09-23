namespace backend.Services.Errors;

public enum ErrorCode
{
    InvalidRefreshToken,
    InvalidCredentials,
    EmailAlreadyTaken,
    AlreadyAssigned,
    OwnAccountDeletion,
    DbOperationFailed,
    ClientNotFound,
    LocationNotFound,
    EmployeeNotFound,
    AssignmentNotFound
}