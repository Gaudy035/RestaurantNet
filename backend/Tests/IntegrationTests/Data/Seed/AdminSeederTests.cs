using backend.Data.Seed;
using IntegrationTests.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace IntegrationTests.Data.Seed;

public class AdminSeederTests: IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    private readonly IConfiguration _config = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Seed:AdminEmail"] = "initadmin@example.com",
            ["Seed:AdminPassword"] = "initAdminPass"
        })
        .Build();

    public AdminSeederTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task SeedInitialAdminAccountCore_WhenNoAdminExists_CreatesAdmin()
    {
        var admin = await DatabaseHelper.ExecuteAsync(_factory, async context =>
        {
            await AdminSeeder.SeedInitialAdminAccountCore(context, _config);

            return await context.Employees.AsNoTracking()
                .Include(e => e.User)
                .FirstOrDefaultAsync(e => e.IsAdmin);
        });

        Assert.NotNull(admin);
        Assert.Equal("initadmin@example.com", admin!.User.Email);
        Assert.True(BCrypt.Net.BCrypt.Verify("initAdminPass", admin.User.Password));
    }

    [Fact]
    public async Task SeedInitialAdminAccountCore_WhenAdminAlreadyExists_DoesNotCreateAnother()
    {
        var emails = await DatabaseHelper.ExecuteAsync(_factory, async context =>
        {
            await UserSeeder.SeedEmployeeUser(context, isAdmin: true, email: "existingadmin@example.com");

            await AdminSeeder.SeedInitialAdminAccountCore(context, _config);

            return await context.Users.AsNoTracking().Select(u => u.Email).ToListAsync();
        });

        Assert.Equal(["existingadmin@example.com"] , emails);
        Assert.DoesNotContain("initadmin@example.com", emails);
    }

    [Fact]
    public async Task SeedInitialAdminAccountCore_WithMissingConfiguration_DoesNotCreateAdmin()
    {
        var emptyConfig = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var adminExists = await DatabaseHelper.ExecuteAsync(_factory, async context =>
        {
            await AdminSeeder.SeedInitialAdminAccountCore(context, emptyConfig);

            return await context.Employees.AnyAsync(e => e.IsAdmin);
        });

        Assert.False(adminExists);
    }
}
