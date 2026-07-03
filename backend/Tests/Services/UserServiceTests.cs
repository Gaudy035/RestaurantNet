using backend.Data;
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
        Assert.Equal(clientResult.UserId, clientResult.User.UserId);
        Assert.NotEqual(clientResult.User.Password, dto.Password);
        Assert.Equal(clientResult.User.Email, dto.Email);
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
        Assert.Equal(resultEmployee.UserId, resultEmployee.User.UserId);
        Assert.NotEqual(resultEmployee.User.Password, dto.Password);
        Assert.Equal(resultEmployee.User.Email, dto.Email);
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
}