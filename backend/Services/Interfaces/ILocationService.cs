using backend.DTOs.Locations;
using backend.Services.Errors;

namespace backend.Services;

public interface ILocationService
{
    Task<Result<LocationResponseDto>> CreateLocation(LocationCreateDto dto);

    Task<Result<IEnumerable<LocationResponseDto>>> GetLocations(string? param);

    Task<Result<LocationResponseDto>> FindLocationById(int locationId);

    Task<Result> DeleteLocation (int locationId);

    Task<Result> AssignEmployee(LocationAssignEmployeeDto dto);

    Task<Result<IEnumerable<LocationEmployeesResponseDto>>> GetAssignedEmployees(int locationId);
    
    Task<Result> UnassignEmployee(LocationAssignEmployeeDto dto);
}