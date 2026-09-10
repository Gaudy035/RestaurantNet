using System.Collections;
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

        return await query.OrderBy(l => l.City)
            .ThenBy(l => l.Address)
            .Select(l => new LocationResponseDto
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

    public async Task<bool> DeleteLocation(int locationId)
    {
        var location = await _context.Locations
            .FindAsync(locationId);

        if (location == null)
        {
            return false;
        }

        _context.Locations.Remove(location);

        try
        {
            await _context.SaveChangesAsync();
        } 
        catch (DbUpdateException)
        {
            return false;
        }
        
        return true;
    }

    public async Task<bool> AssignEmployee(LocationAssignEmployeeDto dto)
    {
        var employeeExists = await _context.Employees.AsNoTracking()
            .AnyAsync(e => e.UserId == dto.UserId);
        
        if (!employeeExists)
        {
            return false;
        }

        var locationExists = await _context.Locations.AsNoTracking()
            .AnyAsync(l => l.LocationId == dto.LocationId);
        
        if (!locationExists)
        {
            return false;
        }

        var alreadyAssigned = await _context.LocationEmployees.AsNoTracking()
            .AnyAsync(le => le.UserId == dto.UserId
                && le.LocationId == dto.LocationId
                && le.Position == dto.Position
            );

        if (alreadyAssigned)
        {
            return false;
        }

        _context.LocationEmployees.Add(new LocationEmployee
        {
            UserId = dto.UserId,
            LocationId = dto.LocationId,
            Position = dto.Position
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return false;
        }

        return true;
    }

    public async Task<IEnumerable<LocationEmployeesResponseDto>> GetAssignedEmployees(int locationId)
    {
        return await _context.LocationEmployees
            .AsNoTracking()
            .Where(le => le.LocationId == locationId)
            .OrderBy(le => le.Position)
            .ThenBy(le => le.Employee.User.LastName)
            .ThenBy(le => le.Employee.User.FirstName)
            .Select(le => new LocationEmployeesResponseDto
            {
                UserId = le.UserId,
                FirstName = le.Employee.User.FirstName,
                LastName = le.Employee.User.LastName,
                LocationId = le.LocationId,
                Position = le.Position
            }).ToListAsync();
    }

    public async Task<bool> UnassignEmployee(LocationAssignEmployeeDto dto)
    {
        var assignment = await _context.LocationEmployees
            .Where(le => le.UserId == dto.UserId 
                && le.LocationId == dto.LocationId
                && le.Position == dto.Position
            ).FirstOrDefaultAsync();
        
        if (assignment == null)
        {
            return false;
        }

        _context.LocationEmployees.Remove(assignment);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return false;
        }

        return true;
    }
}