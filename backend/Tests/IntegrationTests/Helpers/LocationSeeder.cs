using backend.Data;
using backend.Data.Entities;

namespace IntegrationTests.Helpers;

public static class LocationSeeder
{
    public static async Task<Location> SeedLocation(
        AppDbContext context,
        string city = "TestCity",
        string address = "Test Address 42"
    )
    {
        var newLocation = new Location
        {
            City = city,
            Address = address
        };

        context.Locations.Add(newLocation);
        await context.SaveChangesAsync();
        
        return newLocation;
    }

    public static async Task<LocationEmployee> SeedAssignment(
        AppDbContext context,
        int userId,
        int locationId,
        Position position
    )
    {
        var newAssignment = new LocationEmployee
        {
            UserId = userId,
            LocationId = locationId,
            Position = position
        };

        context.LocationEmployees.Add(newAssignment);
        await context.SaveChangesAsync();
        
        return newAssignment;
    }
}