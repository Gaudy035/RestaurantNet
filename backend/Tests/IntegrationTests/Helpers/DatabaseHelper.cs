using backend.Data;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Helpers;

public static class DatabaseHelper
{
    public static async Task<T> ExecuteAsync<T>(
        TestWebApplicationFactory factory, 
        Func<AppDbContext, Task<T>> func)
    {
        using var scope = factory.Services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        return await func(context);
    }
}