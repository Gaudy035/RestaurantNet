namespace backend.Services.Errors;

public enum ErrorCode
{
    InvalidCredentials,
    EmailAlreadyTaken,
    NotFound,
    AlreadyAssigned,
    OwnAccountDeletion,
    DbOperationFailed,
    LocationNotFound,
    EmployeeNotFound
}