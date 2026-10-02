using backend.Data;
using backend.Data.Entities;

namespace UnitTests.Helpers;

public static class LocationSeeder
{
    public static async Task<Location> SeedLocation(
        AppDbContext context,
        string city = "City1",
        string address = "Address1"
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
}