using backend.DTOs.Users;

namespace backend.Services;

public interface IUserService
{
    Task<ClientResponseDto?> CreateClient(CreateClientDto dto);

    Task<EmployeeResponseDto?> CreateEmployee(CreateEmployeeDto dto);

    Task<IEnumerable<ClientResponseDto>> FindClient(string? parameter);

    Task<IEnumerable<EmployeeResponseDto>> FindEmployee(string? parameter);

    Task<bool> DeleteClient(int clientId);
    
    Task<bool> DeleteEmployee(int employeeId);
}