using backend.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace backend.Tests.Helpers;

public static class TestDbContextFactory
{
    public static (AppDbContext context, SqliteConnection connection) Create()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite()
            .Options;
        
        var context = new AppDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }
}