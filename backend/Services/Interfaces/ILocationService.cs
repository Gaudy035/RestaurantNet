using backend.DTOs.Locations;

namespace backend.Services;

public interface ILocationService
{
    Task<LocationResponseDto?> CreateLocation(LocationCreateDto dto);

    Task<IEnumerable<LocationResponseDto>> GetLocations();

    Task<LocationResponseDto?> FindLocation(int locationId);
}