namespace backend.Services.Errors;

public enum ErrorCode
{
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