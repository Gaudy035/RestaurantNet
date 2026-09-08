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
    
    public async Task<IEnumerable<LocationResponseDto>> GetLocations(string? param)
    {
        var query = _context.Locations.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(param))
        {
            var par = $"%{param.ToLower()}%";
            query = query.Where(l => 
                EF.Functions.Like(l.City.ToLower() + " " + l.Address.ToLower(), par) ||
                EF.Functions.Like(l.Address.ToLower() + " " + l.City.ToLower(), par) 
            );
        }

        return await query.Select(l => new LocationResponseDto
        {
            LocationId = l.LocationId,
            City = l.City,
            Address = l.Address
        }).ToListAsync();
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