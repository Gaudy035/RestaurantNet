using backend.Data;
using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

public static class AdminSeeder
{
    public static async Task SeedInitialAdminAccount(IServiceScopeFactory scopeFactory)
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        await SeedInitialAdminAccountCore(context, configuration);
    }
    internal static async Task SeedInitialAdminAccountCore(AppDbContext context, IConfiguration configuration)
    {
        bool adminExists = await context.Employees.AnyAsync(e => e.IsAdmin);
        if (adminExists)
        {
            return;
        }

        var adminEmail = configuration["Seed:AdminEmail"];
        var adminPassword = configuration["Seed:AdminPassword"];

        if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
        {
            return;
        }

        var adminUser = new Employee
        {
            IsAdmin = true,
            User = new User
            {
                FirstName = "System",
                LastName = "Admin",
                Email = adminEmail,
                Password = BCrypt.Net.BCrypt.HashPassword(adminPassword),
            }
        };

        context.Employees.Add(adminUser);

        await context.SaveChangesAsync();
    }
}