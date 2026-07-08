using backend.Data;
using backend.Data.Entities;
using backend.Data.Seed;
using backend.Tests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace backend.Tests.Data.Seed;

public class AdminSeederTests: IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly IConfiguration _config;

    public AdminSeederTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Seed:AdminEmail"] = "initadmin@example.com",
                ["Seed:AdminPassword"] = "initAdminPass"
            })
            .Build();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task SeedExistingAdmin()
    {
        _context.Employees.Add(new Employee
        {
            IsAdmin = true,
            User = new User
            {
                FirstName = "Existing",
                LastName = "Admin",
                Email = "existingadmin@example.com",
                Password = BCrypt.Net.BCrypt.HashPassword("ExistingAdminPassword"),
            }
        });

        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task AdminSeeder_WhenNoAdminFound_CreatesNewAdmin()
    {
        var adminExistsInitially = await _context.Employees.AnyAsync(e => e.IsAdmin);

        Assert.False(adminExistsInitially);

        await AdminSeeder.SeedInitialAdminAccountCore(_context, _config);

        var adminExistsAfterSeederRun = await _context.Employees.AnyAsync(e => e.IsAdmin);
        
        Assert.True(adminExistsAfterSeederRun);
    }

    [Fact]
    public async Task AdminSeeder_WhenAdminAlreadyExists_DoesntCreateNewAdmin()
    {
        await SeedExistingAdmin();

        var adminExistsInitially = await _context.Employees.AnyAsync(e => e.IsAdmin);

        Assert.True(adminExistsInitially);

        await AdminSeeder.SeedInitialAdminAccountCore(_context, _config);

        var adminSeederCreatedAdmin = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == "initadmin@example.com");

        Assert.Null(adminSeederCreatedAdmin);
    }
}