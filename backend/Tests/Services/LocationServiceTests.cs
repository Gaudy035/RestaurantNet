using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Locations;
using backend.Services;
using backend.Tests.Helpers;
using Microsoft.Data.Sqlite;

namespace backend.Tests.Services;

public class LocationSericveTests: IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly LocationService _locationService;

    public LocationSericveTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _locationService = new LocationService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    private async Task<Location> SeedLocation(string city = "City1", string address = "Address1")
    {
        var newLocation = new Location
        {
            City = city,
            Address = address
        };

        _context.Locations.Add(newLocation);
        await _context.SaveChangesAsync();

        return newLocation;
    }

    [Fact]
    public async Task CreateLocation_WithValidData_CreatesLocationAndReturnsItsData()
    {
        var newLocationDto = new LocationCreateDto
        {
            City = "TestCity",
            Address = "TestAddress"
        };

        var result = await _locationService.CreateLocation(newLocationDto);

        Assert.NotNull(result);
        Assert.NotEqual(0, result.LocationId);
        Assert.Equal("TestCity", result.City);
        Assert.Equal("TestAddress", result.Address);
    }

    [Fact]
    public async Task GetLocations_IfLocationsExist_ReturnsAllLocations()
    {
        var location1 = await SeedLocation();
        var location2 = await SeedLocation(city: "City2", address: "Address2");

        var result = await _locationService.GetLocations();

        Assert.Equal(2, result.Count());
        Assert.Contains(result, l => l.City == "City1" && l.Address == "Address1");
        Assert.Contains(result, l => l.City == "City2" && l.Address == "Address2");
    }

    [Fact]
    public async Task GetLocations_WhenNoLocationsExist_ReturnEmptyList()
    {
        var result = await _locationService.GetLocations();

        Assert.Empty(result);
    }

    [Fact]
    public async Task FindLocation_WithCorrectId_ReturnsCorrectLocationData()
    {
        var location = await SeedLocation();

        var result = await _locationService.FindLocation(location.LocationId);

        Assert.NotNull(result);
        Assert.Equal(location.LocationId, result.LocationId);
        Assert.Equal(location.City, result.City);
        Assert.Equal(location.Address, result.Address);
    }

    [Fact]
    public async Task FindLocation_WithIncorrectId_ReturnsNull()
    {
        await SeedLocation();

        var result = await _locationService.FindLocation(42);

        Assert.Null(result);
    }
}   