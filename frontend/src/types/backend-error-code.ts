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
  | 'AssignmentNotFound';

export default BackendErrorCode;
