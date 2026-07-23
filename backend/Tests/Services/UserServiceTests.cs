using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Users;
using backend.Services;
using backend.Tests.Helpers;
using Microsoft.Data.Sqlite;

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

    [Fact]
    public async Task CreateClient_WithValidData_ReturnsClientWithUserId()
    {
        var dto = new CreateClientDto
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
        var dto = new CreateClientDto
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
    public async Task CreateEmployee_WithValidData_ReturnsEmployeeWithUserId()
    {
        var dto = new CreateEmployeeDto
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
        var dto = new CreateEmployeeDto
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

        Assert.NotEmpty(result);
    }
}