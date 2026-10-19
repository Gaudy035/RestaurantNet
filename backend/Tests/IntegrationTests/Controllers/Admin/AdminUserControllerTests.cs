using System.Net;
using System.Net.Http.Json;
using backend.Data.Entities;
using backend.DTOs.Auth;
using backend.DTOs.Users;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Controllers.Admin;

public class AdminUserControllerTests: IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public AdminUserControllerTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public Task InitializeAsync() => _factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    private HttpClient CreateClient()
    {
        return _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                HandleCookies = true
            }
        );
    }

    private async Task<Employee> EmployeeLogin(HttpClient client, bool isAdmin = false)
    {
        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, isAdmin: isAdmin)
        );

        var dto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "password1234"
        };

        await client.PostAsJsonAsync("/admin/auth/login", dto);

        return employee;
    }

    private async Task ClientLogin(HttpClient client)
    {
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );

        var dto = new LoginDto
        {
            Email = "john@example.com",
            Password = "password1234"
        };

        await client.PostAsJsonAsync("auth/login", dto);
    }

    private async Task<Client> SeedClient(string email = "clientemail@example.com", string firstName = "ClientFirstName", string lastName = "ClientLastName") =>
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context, firstName: firstName, lastName: lastName, email: email)
        );

    private async Task<Employee> SeedEmployee(string email = "secondemployee@example.net", string firstName = "Karl", string lastName = "Marx") =>
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, firstName: firstName, lastName: lastName, email: email)
        );

    [Fact]
    public async Task CreateClientAccount_WithLoggedInNonAdminEmployeeUser_CreatesClient()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var createClientDto = new ClientCreateDto
        {
            FirstName = "ClientFirstName",
            LastName = "ClientLastName",
            Email = "clientemail@example.com",
            Password = "password4321",
            PhoneNumber = "123 456 789"
        };

        var response = await client.PostAsJsonAsync("/admin/users/clients", createClientDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.ReadJsonAsync<ClientResponseDto>();
        Assert.True(body.UserId > 0);
        Assert.Equal("ClientFirstName", body.FirstName);
        Assert.Equal("ClientLastName", body.LastName);
        Assert.Equal("clientemail@example.com", body.Email);
        Assert.Equal("123 456 789", body.PhoneNumber);

        var persisted = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => context.Clients.AsNoTracking().Include(c => c.User)
                .FirstOrDefaultAsync(c => c.User.Email == "clientemail@example.com")
        );
        Assert.NotNull(persisted);
        Assert.True(BCrypt.Net.BCrypt.Verify("password4321", persisted!.User.Password));
    }

    [Fact]
    public async Task CreateClientAccount_WithLoggedInAdminEmployeeUser_ReturnsOk()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var createClientDto = new ClientCreateDto
        {
            FirstName = "ClientFirstName",
            LastName = "ClientLastName",
            Email = "clientemail@example.com",
            Password = "password4321",
            PhoneNumber = "123 456 789"
        };

        var response = await client.PostAsJsonAsync("/admin/users/clients", createClientDto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateClientAccount_WithLoggedInEmployeeUserAndDuplicateEmail_ReturnsConflict()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        await SeedClient("john@example.com", "John", "Doe");

        var createClientDto = new ClientCreateDto
        {
            FirstName = "ClientFirstName",
            LastName = "ClientLastName",
            Email = "john@example.com",
            Password = "password4321",
            PhoneNumber = "123 456 789"
        };

        var response = await client.PostAsJsonAsync("/admin/users/clients", createClientDto);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateClientAccount_NoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var createClientDto = new ClientCreateDto
        {
            FirstName = "ClientFirstName",
            LastName = "ClientLastName",
            Email = "john@example.com",
            Password = "password4321",
            PhoneNumber = "123 456 789"
        };

        var response = await client.PostAsJsonAsync("/admin/users/clients", createClientDto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateClientAccount_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var createClientDto = new ClientCreateDto
        {
            FirstName = "ClientFirstName",
            LastName = "ClientLastName",
            Email = "john@example.com",
            Password = "password4321",
            PhoneNumber = "123 456 789"
        };

        var response = await client.PostAsJsonAsync("/admin/users/clients", createClientDto);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetClients_WithLoggedInNonAdminEmployeeUser_ReturnsAllClients()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        await SeedClient("john@example.com", "John", "Zulu");
        await SeedClient("jack@example.com", "Jack", "Alpha");

        var result = await client.GetAsync("admin/users/clients");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var clients = await result.ReadJsonAsync<List<ClientResponseDto>>();
        Assert.Equal(["Jack", "John"], clients.Select(c => c.FirstName));
    }

    [Fact]
    public async Task GetClients_WithSearchParam_ReturnsOnlyMatches()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        await SeedClient("john@example.com", "John", "Doe");
        await SeedClient("jack@example.com", "Jack", "Jackson");

        var result = await client.GetAsync("admin/users/clients?param=ack");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var clients = await result.ReadJsonAsync<List<ClientResponseDto>>();
        Assert.Single(clients);
        Assert.Equal("Jack", clients.Single().FirstName);
        Assert.Equal("jack@example.com", clients.Single().Email);
    }

    [Fact]
    public async Task GetClients_WithLoggedInAdminEmployeeUser_ReturnsOk()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var result = await client.GetAsync("admin/users/clients");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetClients_WithLoggedInClientUser_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var result = await client.GetAsync("admin/users/clients");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetClients_WithNoLoggedInUser_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var result = await client.GetAsync("admin/users/clients");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task GetClientById_WithLoggedInNonAdminEmployeeUserAndCorrectId_ReturnsOk()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var clientUser = await SeedClient("client@example.com", "Jack", "Jackson");

        var result = await client.GetAsync($"admin/users/clients/{clientUser.UserId}");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var body = await result.ReadJsonAsync<ClientResponseDto>();
        Assert.Equal(clientUser.UserId, body.UserId);
        Assert.Equal("Jack", body.FirstName);
        Assert.Equal("Jackson", body.LastName);
        Assert.Equal("client@example.com", body.Email);
    }

    [Fact]
    public async Task GetClientById_WithLoggedInAdminEmployeeUserAndCorrectId_ReturnsOk()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var clientUser = await SeedClient();

        var result = await client.GetAsync($"admin/users/clients/{clientUser.UserId}");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetClientById_WithLoggedInEmployeeUserAndIncorrectId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var result = await client.GetAsync($"admin/users/clients/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetClientById_WithLoggedInClientUser_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var clientUser = await SeedClient("secondclient@example.com");

        var result = await client.GetAsync($"admin/users/clients/{clientUser.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetClientById_WithNoLoggedInUser_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var clientUser = await SeedClient();

        var result = await client.GetAsync($"admin/users/clients/{clientUser.UserId}");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_WithAdminEmployeeUserLoggedInAndCorrectId_RemovesClientAccount()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var clientUser = await SeedClient();

        var result = await client.DeleteAsync($"admin/users/clients/{clientUser.UserId}");

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        var state = await DatabaseHelper.ExecuteAsync(_factory, async context => new
        {
            ClientExists = await context.Clients.AnyAsync(c => c.UserId == clientUser.UserId),
            UserExists = await context.Users.AnyAsync(u => u.UserId == clientUser.UserId)
        });

        Assert.False(state.ClientExists);
        Assert.False(state.UserExists);
    }

    [Fact]
    public async Task DeleteClient_WithAdminEmployeeUserLoggedInAndIncorrectId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var result = await client.DeleteAsync($"admin/users/clients/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var clientUser = await SeedClient();

        var result = await client.DeleteAsync($"admin/users/clients/{clientUser.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var clientUser = await SeedClient("secondclient@example.com");

        var result = await client.DeleteAsync($"admin/users/clients/{clientUser.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var clientUser = await SeedClient();

        var result = await client.DeleteAsync($"admin/users/clients/{clientUser.UserId}");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task CreateEmployeeAccount_WithAdminEmployeeUserLoggedIn_CreatesEmployee()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var dto = new EmployeeCreateDto
        {
            FirstName = "Employee",
            LastName = "User",
            Email = "employee@example.com",
            Password = "password",
            IsAdmin = false
        };

        var response = await client.PostAsJsonAsync($"admin/users/employees", dto);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.ReadJsonAsync<EmployeeResponseDto>();
        Assert.True(body.UserId > 0);
        Assert.Equal("Employee", body.FirstName);
        Assert.Equal("User", body.LastName);
        Assert.Equal("employee@example.com", body.Email);
        Assert.False(body.IsAdmin);

        var persisted = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => context.Employees.AsNoTracking().Include(e => e.User)
                .FirstOrDefaultAsync(e => e.User.Email == "employee@example.com")
        );
        Assert.NotNull(persisted);
        Assert.True(BCrypt.Net.BCrypt.Verify("password", persisted!.User.Password));
    }

    [Fact]
    public async Task CreateEmployeeAccount_WithAdminEmployeeUserLoggedInAndDuplicateEmail_ReturnsConflict()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        await SeedClient("employee@example.com");

        var dto = new EmployeeCreateDto
        {
            FirstName = "Employee",
            LastName = "User",
            Email = "employee@example.com",
            Password = "password",
            IsAdmin = false
        };

        var response = await client.PostAsJsonAsync($"admin/users/employees", dto);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployeeAccount_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var dto = new EmployeeCreateDto
        {
            FirstName = "Employee",
            LastName = "User",
            Email = "employee@example.com",
            Password = "password",
            IsAdmin = false
        };

        var response = await client.PostAsJsonAsync($"admin/users/employees", dto);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployeeAccount_WithClientLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var dto = new EmployeeCreateDto
        {
            FirstName = "Employee",
            LastName = "User",
            Email = "employee@example.com",
            Password = "password",
            IsAdmin = false
        };

        var response = await client.PostAsJsonAsync($"admin/users/employees", dto);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateEmployeeAccount_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var dto = new EmployeeCreateDto
        {
            FirstName = "Employee",
            LastName = "User",
            Email = "employee@example.com",
            Password = "password",
            IsAdmin = false
        };

        var response = await client.PostAsJsonAsync($"admin/users/employees", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_WithAdminEmployeeUserLoggedIn_ReturnsAllEmployees()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        await SeedEmployee("karl@example.com", "Karl", "Marx");
        await SeedEmployee("ada@example.com", "Ada", "Lovelace");

        var result = await client.GetAsync($"admin/users/employees");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var employees = await result.ReadJsonAsync<List<EmployeeResponseDto>>();
        Assert.Equal(3, employees.Count);
        Assert.Contains(employees, e => e.Email == "karl@example.com");
        Assert.Contains(employees, e => e.Email == "ada@example.com");
    }

    [Fact]
    public async Task GetEmployees_WithSearchParam_ReturnsOnlyMatches()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        await SeedEmployee("karl@example.com", "Karl", "Marx");
        await SeedEmployee("ada@example.com", "Ada", "Lovelace");

        var result = await client.GetAsync($"admin/users/employees?param=lovelace");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var employees = await result.ReadJsonAsync<List<EmployeeResponseDto>>();
        Assert.Single(employees);
        Assert.Equal("Ada", employees.Single().FirstName);
    }

    [Fact]
    public async Task GetEmployees_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var result = await client.GetAsync($"admin/users/employees");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var result = await client.GetAsync($"admin/users/employees");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var result = await client.GetAsync($"admin/users/employees");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployeeById_WithAdminEmployeeUserLoggedInAndCorrectId_ReturnsEmployee()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employeeUser = await SeedEmployee("karl@example.com", "Karl", "Marx");

        var result = await client.GetAsync($"admin/users/employees/{employeeUser.UserId}");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var body = await result.ReadJsonAsync<EmployeeResponseDto>();
        Assert.Equal(employeeUser.UserId, body.UserId);
        Assert.Equal("Karl", body.FirstName);
        Assert.Equal("Marx", body.LastName);
        Assert.Equal("karl@example.com", body.Email);
        Assert.False(body.IsAdmin);
    }

    [Fact]
    public async Task GetEmployeeById_WithAdminEmployeeUserLoggedInAndIncorrectId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var result = await client.GetAsync($"admin/users/employees/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployeeById_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var employeeUser = await SeedEmployee();

        var result = await client.GetAsync($"admin/users/employees/{employeeUser.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployeeById_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var employeeUser = await SeedEmployee();

        var result = await client.GetAsync($"admin/users/employees/{employeeUser.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployeeById_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var employeeUser = await SeedEmployee();

        var result = await client.GetAsync($"admin/users/employees/{employeeUser.UserId}");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployeeLocations_WithAdminEmployeeUserLoggedInAndCorrectId_ReturnsLocations()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employee = await SeedEmployee();
        var location1 = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context, "City1", "Address1")
        );
        var location2 = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context, "City2", "Address2")
        );
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employee.UserId, location1.LocationId, Position.Server)
        );
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employee.UserId, location2.LocationId, Position.Driver)
        );

        var result = await client.GetAsync($"admin/users/employees/{employee.UserId}/locations");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var locations = await result.ReadJsonAsync<List<EmployeeLocationsResponseDto>>();
        Assert.Equal(2, locations.Count);
        Assert.Contains(locations, l => l.City == "City1" && l.Position == Position.Server && l.UserId == employee.UserId);
        Assert.Contains(locations, l => l.City == "City2" && l.Position == Position.Driver);
    }

    [Fact]
    public async Task GetEmployeeLocations_WithAdminEmployeeUserLoggedInAndIncorrectId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var result = await client.GetAsync($"admin/users/employees/{int.MaxValue}/locations");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployeeLocations_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var employee = await SeedEmployee();

        var result = await client.GetAsync($"admin/users/employees/{employee.UserId}/locations");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployeeLocations_WithClientLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var employee = await SeedEmployee();

        var result = await client.GetAsync($"admin/users/employees/{employee.UserId}/locations");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployeeLocations_WithNoLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var employee = await SeedEmployee();

        var result = await client.GetAsync($"admin/users/employees/{employee.UserId}/locations");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_WithAdminEmployeeUserLoggedInAndCorrectId_RemovesEmployeeAccount()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employeeUser = await SeedEmployee();

        var result = await client.DeleteAsync($"admin/users/employees/{employeeUser.UserId}");

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        var state = await DatabaseHelper.ExecuteAsync(_factory, async context => new
        {
            EmployeeExists = await context.Employees.AnyAsync(e => e.UserId == employeeUser.UserId),
            UserExists = await context.Users.AnyAsync(u => u.UserId == employeeUser.UserId)
        });

        Assert.False(state.EmployeeExists);
        Assert.False(state.UserExists);
    }

    [Fact]
    public async Task DeleteEmployee_WithAdminEmployeeUserLoggedInAndOwnId_ReturnsBadRequest()
    {
        var client = CreateClient();

        var admin = await EmployeeLogin(client, true);

        var result = await client.DeleteAsync($"admin/users/employees/{admin.UserId}");

        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_WithAdminEmployeeUserLoggedInAndIncorrectId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var result = await client.DeleteAsync($"admin/users/employees/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var employeeUser = await SeedEmployee();

        var result = await client.DeleteAsync($"admin/users/employees/{employeeUser.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var employeeUser = await SeedEmployee();

        var result = await client.DeleteAsync($"admin/users/employees/{employeeUser.UserId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var employeeUser = await SeedEmployee();

        var result = await client.DeleteAsync($"admin/users/employees/{employeeUser.UserId}");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}
