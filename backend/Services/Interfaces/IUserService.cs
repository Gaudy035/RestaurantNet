using backend.DTOs.Users;

namespace backend.Services;

public interface IUserService
{
    Task<ClientResponseDto?> CreateClient(ClientCreateDto dto);

    Task<IEnumerable<ClientResponseDto>> FindClient(string? parameter);

    Task<ClientResponseDto?> FindClientById(int clientId);

    Task<bool> DeleteClient(int clientId);

    Task<EmployeeResponseDto?> CreateEmployee(EmployeeCreateDto dto);

    Task<IEnumerable<EmployeeResponseDto>> FindEmployee(string? parameter);

    Task<EmployeeResponseDto?> FindEmployeeById(int employeeId);

    Task<IEnumerable<EmployeeLocationsResponseDto>> GetEmployeeLocations(int employeeId);
    
    Task<bool> DeleteEmployee(int employeeId);
}