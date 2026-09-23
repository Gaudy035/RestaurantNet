using backend.DTOs.Users;
using backend.Services.Errors;

namespace backend.Services;

public interface IUserService
{
    Task<Result<ClientResponseDto>> CreateClient(ClientCreateDto dto);

    Task<Result<IEnumerable<ClientResponseDto>>> FindClient(string? parameter);

    Task<Result<ClientResponseDto>> FindClientById(int clientId);

    Task<bool> DeleteClient(int clientId);

    Task<EmployeeResponseDto?> CreateEmployee(EmployeeCreateDto dto);

    Task<IEnumerable<EmployeeResponseDto>> FindEmployee(string? parameter);

    Task<EmployeeResponseDto?> FindEmployeeById(int employeeId);

    Task<IEnumerable<EmployeeLocationsResponseDto>> GetEmployeeLocations(int employeeId);
    
    Task<bool> DeleteEmployee(int employeeId);
}