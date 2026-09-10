using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Users;
using backend.Services;
using backend.Tests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace backend.Tests.Services;

public class UserServiceTests: IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _userService = new UserService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
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

    private async Task<Employee> SeedEmployeeUser(string firstName = "Jane", string lastName = "Doe", string email = "jane@example.com", string password = "password1234", bool isAdmin = false)
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

    private async Task<LocationEmployee> SeedAssignment(int userId, int locationId, Position position)
    {
        var newAssignment = new LocationEmployee
        {
            UserId = userId,
            LocationId = locationId,
            Position = position
        };

        _context.LocationEmployees.Add(newAssignment);
        await _context.SaveChangesAsync();

        return newAssignment;
    }
    
    [Fact]
    public async Task CreateClient_WithValidData_ReturnsClientWithUserId()
    {
        var dto = new ClientCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "password1234",
            PhoneNumber = "123 456 789"
        };

        var clientResult = await _userService.CreateClient(dto);

        Assert.NotNull(clientResult);
        Assert.True(clientResult.UserId > 0);
        Assert.Equal(clientResult.Email, dto.Email);
        Assert.Equal(clientResult.PhoneNumber, dto.PhoneNumber);
    }
    
    [Fact]
    public async Task CreateClient_WithDuplicateEmail_ReturnsNull()
    {
        var dto = new ClientCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "password1234",
            PhoneNumber = "123 456 789"
        };

        // First client with said email
        await _userService.CreateClient(dto);
        var secondClient = await _userService.CreateClient(dto);

        Assert.Null(secondClient);
    }

    [Fact]
    public async Task FindClient_WithNoParameter_ReturnsAllClients()
    {
        await SeedClientUser();
        await SeedClientUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");

        var result = await _userService.FindClient(null);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FindClient_WithProperName_ReturnsRightClient()
    {
        await SeedClientUser();
        await SeedClientUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");

        var result = await _userService.FindClient("ack");

        Assert.Single(result);

        var client = result.Single();

        Assert.Equal("Jack", client.FirstName);
        Assert.Equal("Jackson", client.LastName);
        Assert.Equal("jack@example.com", client.Email);
    }

    [Fact]
    public async Task FindClient_WithMultipleMatches_ReturnsMultipleClients()
    {
        var client1 = await SeedClientUser();
        var client2 = await SeedClientUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");
        var client3 = await SeedClientUser(firstName: "John", lastName: "Jackson", email: "john2@example.com");

        var result = await _userService.FindClient("john");

        Assert.Equal(2, result.Count());
        Assert.Contains(result, c => c.UserId == client1.UserId);
        Assert.DoesNotContain(result, c => c.UserId == client2.UserId);
        Assert.Contains(result, c => c.UserId == client3.UserId);
    }

    [Fact]
    public async Task FindClient_WithMatchingCombinedNameString_ReturnsRightClient()
    {
        var client1 = await SeedClientUser();
        var client2 = await SeedClientUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");

        var result = await _userService.FindClient("john doe");

        Assert.Single(result);
        Assert.Contains(result, c => c.UserId == client1.UserId);
        Assert.DoesNotContain(result, c => c.UserId == client2.UserId);
    }

    [Fact]
    public async Task FindClient_WithoutMatches_ReturnsEmpty()
    {
        await SeedClientUser();
        await SeedClientUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");

        var result = await _userService.FindClient("someRandomParameter");

        Assert.Empty(result);
    }

    [Fact]
    public async Task FindClient_WithMatchInEmployees_ReturnsEmpty()
    {
        var employee = await SeedEmployeeUser();

        var result = await _userService.FindClient(employee.User.FirstName);

        Assert.Empty(result);
    }

    [Fact]
    public async Task DeleteClient_WithCorrectId_DeletesAndReturnsTrue()
    {
        var client = await SeedClientUser();

        var countBefore = await _context.Clients.CountAsync();

        Assert.Equal(1, countBefore);

        var result = await _userService.DeleteClient(client.UserId);
        var countAfter = await _context.Users.CountAsync();
        var countAfter2 = await _context.Clients.CountAsync();

        Assert.True(result);
        Assert.Equal(0, countAfter);
        Assert.Equal(0, countAfter2);
    }

    [Fact]
    public async Task DeleteClient_WithNoMatch_DoesntDeleteAndReturnsFalse()
    {
        await SeedClientUser();

        var countBefore = await _context.Clients.CountAsync();

        Assert.Equal(1, countBefore);

        var result = await _userService.DeleteClient(99);
        var countAfter = await _context.Users.CountAsync();
        var countAfter2 = await _context.Clients.CountAsync();

        Assert.False(result);
        Assert.Equal(countBefore, countAfter);
        Assert.Equal(countBefore, countAfter2);
    }

    [Fact]
    public async Task DeleteClient_WithMatchInEmployees_DoesntDeleteAndReturnsFalse()
    {
        var employee = await SeedEmployeeUser();

        var countBefore = await _context.Users.CountAsync();

        Assert.Equal(1, countBefore);

        var result = await _userService.DeleteClient(employee.UserId);
        var countAfter = await _context.Users.CountAsync();

        Assert.False(result);
        Assert.Equal(countBefore, countAfter);
    }

    [Fact]
    public async Task CreateEmployee_WithValidData_ReturnsEmployeeWithUserId()
    {
        var dto = new EmployeeCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "password1234",
            IsAdmin = false
        };

        var resultEmployee = await _userService.CreateEmployee(dto);

        Assert.NotNull(resultEmployee);
        Assert.True(resultEmployee.UserId > 0);
        Assert.Equal(resultEmployee.Email, dto.Email);
        Assert.Equal(resultEmployee.IsAdmin, dto.IsAdmin);
    }

    [Fact]
    public async Task CreateEmployee_WithDuplicateEmail_ReturnsNull()
    {
        var dto = new EmployeeCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Password = "password1234",
            IsAdmin = false
        };

        // First Employee with said email
        await _userService.CreateEmployee(dto);
        var secondEmployee = await _userService.CreateEmployee(dto);

        Assert.Null(secondEmployee);
    }

    [Fact]
    public async Task FindEmployee_WithNoParameter_ReturnsAllEmployees()
    {
        await SeedEmployeeUser();
        await SeedEmployeeUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");
        
        var result = await _userService.FindEmployee(null);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task FindEmployee_WithProperName_ReturnsRightEmployee()
    {
        await SeedEmployeeUser();
        await SeedEmployeeUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");

        var result = await _userService.FindEmployee("ack");

        Assert.Single(result);
        
        var employee = result.Single();

        Assert.Equal("Jack", employee.FirstName);
        Assert.Equal("Jackson", employee.LastName);
        Assert.Equal("jack@example.com", employee.Email);
    }

    [Fact]
    public async Task FindEmployee_WithMultipleMatches_ReturnsAllMatchingEmployees()
    {
        var employee1 = await SeedEmployeeUser();
        var employee2 = await SeedEmployeeUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");
        var employee3 = await SeedEmployeeUser(firstName: "John", lastName: "Doe", email: "john@example.com");

        var result = await _userService.FindEmployee("doe");

        Assert.Equal(2, result.Count());
        Assert.Contains(result, e => e.UserId == employee1.UserId);
        Assert.DoesNotContain(result, e => e.UserId == employee2.UserId);
        Assert.Contains(result, e => e.UserId == employee3.UserId);
    }

    [Fact]
    public async Task FindEmployee_WithMatchingCombinedNameString_ReturnsRightEmployee()
    {
        var employee1 = await SeedEmployeeUser();
        var employee2 = await SeedEmployeeUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");

        var result = await _userService.FindEmployee("jane doe");

        Assert.Single(result);
        Assert.Contains(result, e => e.UserId == employee1.UserId);
        Assert.DoesNotContain(result, e => e.UserId == employee2.UserId);
    }

    [Fact]
    public async Task FindEmployee_WithoutMatches_ReturnsEmpty()
    {
        await SeedEmployeeUser();
        await SeedEmployeeUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");

        var result = await _userService.FindEmployee("someRandomParameter");

        Assert.Empty(result);
    }

    [Fact]
    public async Task FindEmployee_WithMatchInClients_ReturnsEmpty()
    {
        var client = await SeedClientUser();

        var result = await _userService.FindEmployee(client.User.FirstName);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEmployeeLocations_WithMultipleMatches_ReturnsAllMatches()
    {
        var employee = await SeedEmployeeUser();
        var location1 = await SeedLocation();
        var location2 = await SeedLocation(city: "City 2", address: "Address 2");
        await SeedAssignment(employee.UserId, location1.LocationId, Position.Server);
        await SeedAssignment(employee.UserId, location1.LocationId, Position.Cashier);
        await SeedAssignment(employee.UserId, location2.LocationId, Position.Driver);

        var assignmentCount = await _context.LocationEmployees.CountAsync();
        var result = await _userService.GetEmployeeLocations(employee.UserId);

        Assert.Equal(3, assignmentCount);
        Assert.Equal(assignmentCount, result.Count());
        Assert.Contains(result, x => 
            x.LocationId == location1.LocationId &&
            x.City == location1.City &&
            x.Address == location1.Address &&
            x.UserId == employee.UserId &&
            x.Position == Position.Server
        );
        Assert.Contains(result, x => 
            x.LocationId == location1.LocationId &&
            x.City == location1.City &&
            x.Address == location1.Address &&
            x.UserId == employee.UserId &&
            x.Position == Position.Cashier
        );
        Assert.Contains(result, x => 
            x.LocationId == location2.LocationId &&
            x.City == location2.City &&
            x.Address == location2.Address &&
            x.UserId == employee.UserId &&
            x.Position == Position.Driver
        );
    }

    [Fact]
    public async Task GetEmployeeLocations_WithNoMatches_ReturnsEmptyList()
    {
        var employee = await SeedEmployeeUser();

        var result = await _userService.GetEmployeeLocations(employee.UserId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetEmployeeLocations_WithMatches_ReturnsOnlyThoseWithRightEmployee()
    {
        var employee1 = await SeedEmployeeUser();
        var employee2 = await SeedEmployeeUser(firstName: "Jack", lastName: "Jackson", email: "jack@example.com");
        var location = await SeedLocation();
        await SeedAssignment(employee1.UserId, location.LocationId, Position.Chef);
        await SeedAssignment(employee1.UserId, location.LocationId, Position.Server);
        await SeedAssignment(employee2.UserId, location.LocationId, Position.Chef);

        var countAssigned = await _context.LocationEmployees.CountAsync();
        var result = await _userService.GetEmployeeLocations(employee1.UserId);

        Assert.Equal(3, countAssigned);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteEmployee_WithCorrectId_DeletesAndReturnsTrue()
    {
        var employee = await SeedEmployeeUser();

        var countBefore = await _context.Employees.CountAsync();

        Assert.Equal(1, countBefore);

        var result = await _userService.DeleteEmployee(employee.UserId);
        var countAfter = await _context.Users.CountAsync();
        var countAfter2 = await _context.Employees.CountAsync();
        
        Assert.True(result);
        Assert.Equal(0, countAfter);
        Assert.Equal(0, countAfter2);
    }

    [Fact]
    public async Task DeleteEmployee_WithNoMatch_DoesntDeleteAndReturnsFalse()
    {
        var employee = await SeedEmployeeUser();

        var countBefore = await _context.Employees.CountAsync();

        Assert.Equal(1, countBefore);

        var result = await _userService.DeleteEmployee(99);
        var countAfter = await _context.Users.CountAsync();
        var countAfter2 = await _context.Employees.CountAsync();

        Assert.False(result);
        Assert.Equal(countBefore, countAfter);
        Assert.Equal(countBefore, countAfter2);
    }

     [Fact]
    public async Task DeleteEmployee_WithMatchInClients_DoesntDeleteAndReturnsFalse()
    {
        var client = await SeedClientUser();

        var countBefore = await _context.Users.CountAsync();

        Assert.Equal(1, countBefore);

        var result = await _userService.DeleteEmployee(client.UserId);
        var countAfter = await _context.Users.CountAsync();

        Assert.False(result);
        Assert.Equal(countBefore, countAfter);
    }
}