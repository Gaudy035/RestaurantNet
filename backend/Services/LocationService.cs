using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Locations;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class LocationService : ILocationService
{
    private readonly AppDbContext _context;

    public LocationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LocationResponseDto?> CreateLocation(LocationCreateDto dto)
    {
        var newLocation = new Location
        {
            City = dto.City,
            Address = dto.Address
        };

        _context.Locations.Add(newLocation);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return new LocationResponseDto
        {
            LocationId = newLocation.LocationId,
            City = newLocation.City,
            Address = newLocation.Address
        };
    }
    
    public async Task<IEnumerable<LocationResponseDto>> GetLocations()
    {
        var locations = await _context.Locations.AsNoTracking()
            .Select(l => new LocationResponseDto
            {
                LocationId = l.LocationId,
                City = l.City,
                Address = l.Address
            }).ToListAsync();

        return locations;
    }

    public async Task<LocationResponseDto?> FindLocation(int locationId)
    {
        var location = await _context.Locations.AsNoTracking()
            .Where(l => l.LocationId == locationId)
            .Select(l => new LocationResponseDto
            {
                LocationId = l.LocationId,
                City = l.City,
                Address = l.Address
            }).FirstOrDefaultAsync();
            
        return location;
    }
}