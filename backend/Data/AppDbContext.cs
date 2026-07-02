using backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options): base(options){}

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<LocationEmployee> LocationEmployees => Set<LocationEmployee>();
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<ItemIngredient> ItemIngredients => Set<ItemIngredient>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Delivery> Deliveries => Set<Delivery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}