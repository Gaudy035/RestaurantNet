type BackendErrorCode =
    | 'InvalidRefreshToken'
    | 'InvalidCredentials'
    | 'EmailAlreadyTaken'
    | 'AlreadyAssigned'
    | 'OwnAccountDeletion'
    | 'DbOperationFailed'
    | 'ClientNotFound'
    | 'LocationNotFound'
    | 'EmployeeNotFound'
    | 'AssignmentNotFound'
    | 'CategoryNotFound'
    | 'CategoryAlreadyExists'
    | 'CategoryContainsItems'
    | 'MenuItemNotFound'
    | 'UpdateBodyEmpty'

export default BackendErrorCode;
