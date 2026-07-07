using backend.Data;
using backend.Data.Entities;
using backend.Services;
using backend.Tests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace backend.Tests.Services;

public class AuthServiceTests: IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();

        var _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "very-long-jwt-security-key-for-testing-auth-service-minimum=thirty-two-charatcters-long"
            })
            .Build();

        _authService = new AuthService(_context, _config);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<Client> SeedClientUser()
    {
        var newClient = new Client
        {
            PhoneNumber = "123 123 123",
            User = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("TestPass1234"),
            }
        };

        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        return newClient;
    }

    private async Task<Employee> SeedEmployeeUser(bool isAdmin)
    {
        var newEmployee = new Employee
        {
            IsAdmin = isAdmin,
            User = new User
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("TestPass1234"),
            }
        };

        _context.Employees.Add(newEmployee);
        await _context.SaveChangesAsync();

        return newEmployee;
    }


}