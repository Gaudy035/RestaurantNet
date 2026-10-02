using backend.Data;
using backend.Data.Entities;

namespace UnitTests.Helpers;

public static class ClientUserSeeder
{
    public static async Task<Client> SeedClientUser(
        AppDbContext context,
        string firstName = "John", 
        string lastName = "Doe",
        string email = "john@example.com",
        string password = "password1234",
        string phoneNumber = "123 456 789"
        )
    {
        var newClient = new Client
        {
            PhoneNumber = phoneNumber,
            User = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            }
        };

        context.Clients.Add(newClient);
        await context.SaveChangesAsync();

        return newClient;
    }
}