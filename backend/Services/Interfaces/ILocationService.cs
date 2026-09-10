using backend.DTOs.Locations;

namespace backend.Services;

public interface ILocationService
{
    Task<LocationResponseDto?> CreateLocation(LocationCreateDto dto);

    Task<IEnumerable<LocationResponseDto>> GetLocations(string? param);

    Task<LocationResponseDto?> FindLocation(int locationId);

    Task<bool> DeleteLocation (int locationId);

    Task<bool> AssignEmployee(LocationAssignEmployeeDto dto);

    Task<IEnumerable<LocationEmployeesResponseDto>> GetAssignedEmployees(int locationId);
    
    Task<bool> UnassignEmployee(LocationAssignEmployeeDto dto);
}