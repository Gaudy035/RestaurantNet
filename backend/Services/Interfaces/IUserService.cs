using backend.DTOs.Users;
using backend.Services.Errors;

namespace backend.Services;

public interface IUserService
{
    Task<Result<ClientResponseDto>> CreateClient(ClientCreateDto dto);

    Task<Result<IEnumerable<ClientResponseDto>>> FindClient(string? parameter);

    Task<Result<ClientResponseDto>> FindClientById(int clientId);

    Task<Result> DeleteClient(int clientId);

    Task<Result<EmployeeResponseDto>> CreateEmployee(EmployeeCreateDto dto);

    Task<Result<IEnumerable<EmployeeResponseDto>>> FindEmployee(string? parameter);

    Task<Result<EmployeeResponseDto>> FindEmployeeById(int employeeId);

    Task<Result<IEnumerable<EmployeeLocationsResponseDto>>> GetEmployeeLocations(int employeeId);
    
    Task<Result> DeleteEmployee(int employeeId, int currentAdminId);
}