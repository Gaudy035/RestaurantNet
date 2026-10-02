namespace UnitTests.Helpers;
using backend.Data;
using backend.Data.Entities;

public static class AssignmentSeeder
{
    
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