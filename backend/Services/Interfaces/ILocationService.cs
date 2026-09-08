using backend.DTOs.Locations;

namespace backend.Services;

public interface ILocationService
{
    Task<LocationResponseDto?> CreateLocation(LocationCreateDto dto);
}