using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Locations;
using backend.Services;
using backend.Tests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

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

    private async Task<Client> SeedClientUser(string firstName = "John", string lastName = "Doe", string email = "john@example.com", string password = "password1234", string phoneNumber = "123 456 789")
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

        _context.Clients.Add(newClient);
        await _context.SaveChangesAsync();

        return newClient;
    }

    private async Task<Employee> SeedEmployeeUser(string firstName = "John", string lastName = "Doe", string email = "jane@example.com", string password = "password1234", bool isAdmin = false)
    {
        var newEmployee = new Employee
        {
            IsAdmin = isAdmin,
            User = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = BCrypt.Net.BCrypt.HashPassword(password)
            }
        };

        _context.Employees.Add(newEmployee);
        await _context.SaveChangesAsync();

        return newEmployee;
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
    public async Task GetLocations_WhenNoLocationsExist_ReturnEmpty()
    {
        var result = await _locationService.GetLocations(null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLocations_WithNoParam_ReturnsAllLocations()
    {
        var location1 = await SeedLocation();
        var location2 = await SeedLocation(city: "City2", address: "Address2");

        var result = await _locationService.GetLocations(null);

        Assert.Equal(2, result.Count());
        Assert.Contains(result, l => l.City == "City1" && l.Address == "Address1");
        Assert.Contains(result, l => l.City == "City2" && l.Address == "Address2");
    }

    [Fact]
    public async Task GetLocation_WithCorrectParam_ReturnsAllMatches()
    {
        var location1 = await SeedLocation();
        var location2 = await SeedLocation(city: "City2", address: "Address2");
        var location3 = await SeedLocation(city: "City3", address: "Address2");

        var result = await _locationService.GetLocations("ess2");

        Assert.Equal(2, result.Count());
        Assert.Contains(result, l => l.City == "City2" && l.Address == "Address2");
        Assert.Contains(result, l => l.City == "City3" && l.Address == "Address2");
        Assert.DoesNotContain(result, l => l.City == "City1" && l.Address == "Address1");
    }

    [Fact]
    public async Task GetLocation_WithNoMatches_ReturnEmpty()
    {
        await SeedLocation();

        var result = await _locationService.GetLocations("NonExistantAddress");

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

    [Fact]
    public async Task AssignEmployee_WithCorrectData_AssignsEmployeeAndReturnsTrue()
    {
        var employee = await SeedEmployeeUser();
        var location = await SeedLocation();

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var result = await _locationService.AssignEmployee(dto);
        var assignedEmployees = await _context.LocationEmployees
            .Where(le => le.Location == location
                && le.Employee == employee
                && le.Position == dto.Position
            )
            .CountAsync();

        Assert.True(result);
        Assert.Equal(1, assignedEmployees);
        
    }

    [Fact]
    public async Task AssignEmployee_WithClientId_DoesntAssignAndReturnsFalse()
    {
        var client = await SeedClientUser();
        var location = await SeedLocation();

        var dto = new LocationAssignEmployeeDto
        {
            UserId = client.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var result = await _locationService.AssignEmployee(dto);
        var assignedEmployees = await _context.LocationEmployees.CountAsync();

        Assert.False(result);
        Assert.Equal(0, assignedEmployees);
    }

    [Fact]
    public async Task AssignEmployee_WithNonExistentUser_DoesntAssignAndReturnsFalse()
    {
        var location = await SeedLocation();

        var dto = new LocationAssignEmployeeDto
        {
            UserId = 42,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var result = await _locationService.AssignEmployee(dto);
        var assignedEmployees = await _context.LocationEmployees.CountAsync();

        Assert.False(result);
        Assert.Equal(0, assignedEmployees);
    }

    [Fact]
    public async Task AssignEmployee_WithNonExistentLocation_DoesntAssignAndReturnsFalse()
    {
        var employee = await SeedEmployeeUser();

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = 42,
            Position = Position.Cashier
        };

        var result = await _locationService.AssignEmployee(dto);
        var assignedEmployees = await _context.LocationEmployees.CountAsync();

        Assert.False(result);
        Assert.Equal(0, assignedEmployees);
    }

    [Fact]
    public async Task AssignEmployee_WithDuplicateData_DoesntAssignAndReturnsFalse()
    {
        var employee = await SeedEmployeeUser();
        var location = await SeedLocation();

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var result1 = await _locationService.AssignEmployee(dto);
        var result2 = await _locationService.AssignEmployee(dto);
        var assignedEmployees = await _context.LocationEmployees.CountAsync();

        Assert.True(result1);
        Assert.False(result2);
        Assert.Equal(1, assignedEmployees);
    }

    [Fact]
    public async Task AssignEmployee_WithSameIdsAndDifferentPosition_AssignsAndReturnsTrue()
    {
        var employee = await SeedEmployeeUser();
        var location = await SeedLocation();

        var dto1 = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };
        
        var dto2 = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Chef
        };

        var result1 = await _locationService.AssignEmployee(dto1);
        var result2 = await _locationService.AssignEmployee(dto2);
        var assignedEmployees = await _context.LocationEmployees.CountAsync();

        Assert.True(result1);
        Assert.True(result2);
        Assert.Equal(2, assignedEmployees);
    }
}   