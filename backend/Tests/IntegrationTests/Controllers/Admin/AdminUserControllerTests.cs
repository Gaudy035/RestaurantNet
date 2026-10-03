using System.Net;
using System.Net.Http.Json;
using backend.DTOs.Auth;
using backend.DTOs.Users;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

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

    private async Task EmployeeLogin(HttpClient client, bool isAdmin = false)
    {
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, isAdmin: isAdmin)
        );

        var dto = new LoginDto
        {
            Email = "jane@example.com",
            Password = "password1234"
        };
        
        await client.PostAsJsonAsync("/admin/auth/login", dto);
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
    
    [Fact]
    public async Task CreateClientAccount_WithLoggedInNonAdminEmployeeUser_ReturnsOk()
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

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );
        
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
    public async Task GetClients_WithLoggedInNonAdminEmployeeUser_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client);
        
        var result = await client.GetAsync("admin/users/clients");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
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
        
        var clientUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );
        
        var result = await client.GetAsync($"admin/users/clients/{clientUser.UserId}");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task GetClientById_WithLoggedInAdminEmployeeUserAndCorrectId_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);
        
        var clientUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );
        
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
        
        var result = await client.GetAsync("admin/users/clients");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetClientById_WithNoLoggedInUser_ReturnsUnauthorized()
    {
        var client = CreateClient();
        
        var result = await client.GetAsync("admin/users/clients");
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
    
    [Fact]
    public async Task DeleteClient_WithAdminEmployeeUserLoggedInAndCorrectId_ReturnsNoContent()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var clientUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );
        
        var result = await client.DeleteAsync($"admin/users/clients/{clientUser.UserId}");
        
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
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

        var clientUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );
        
        var result = await client.DeleteAsync($"admin/users/clients/{clientUser.UserId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task DeleteClient_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);
        
        var clientUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context, email: "secondclient@example.com")
        );
        
        var result = await client.DeleteAsync($"admin/users/clients/{clientUser.UserId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task DeleteClient_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();
        
        var clientUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context)
        );
        
        var result = await client.DeleteAsync($"admin/users/clients/{clientUser.UserId}");
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateEmployeeAccount_WithAdminEmployeeUserLoggedIn_ReturnsOk()
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
    }    
    
    [Fact]
    public async Task CreateEmployeeAccount_WithAdminEmployeeUserLoggedInAndDuplicateEmail_ReturnsConflict()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedClientUser(context, email: "employee@example.com")
        );
        
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
    public async Task GetEmployees_WithAdminEmployeeUserLoggedIn_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);
        
        var result = await client.GetAsync($"admin/users/employees");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
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
    public async Task GetEmployeeById_WithAdminEmployeeUserLoggedInAndCorrectId_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var employeeUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.net")
        );
        
        var result = await client.GetAsync($"admin/users/employees/{employeeUser.UserId}");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
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

        var employeeUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.net")
        );
        
        var result = await client.GetAsync($"admin/users/employees/{employeeUser.UserId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task GetEmployeeById_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var employeeUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context)
        );
        
        var result = await client.GetAsync($"admin/users/employees/{employeeUser.UserId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task GetEmployeeById_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();
        
        var employeeUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context)
        );
        
        var result = await client.GetAsync($"admin/users/employees/{employeeUser.UserId}");
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task GetEmployeeLocations_WithAdminEmployeeUserLoggedInAndCorrectId_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );
        
        var result = await client.GetAsync($"admin/users/employees/{employee.UserId}/locations");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
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

        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );
        
        var result = await client.GetAsync($"admin/users/employees/{employee.UserId}/locations");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task GetEmployeeLocations_WithClientLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );
        
        var result = await client.GetAsync($"admin/users/employees/{employee.UserId}/locations");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task GetEmployeeLocations_WithNoLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();
        
        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );
        
        var result = await client.GetAsync($"admin/users/employees/{employee.UserId}/locations");
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
    
    [Fact]
    public async Task DeleteEmployee_WithAdminEmployeeUserLoggedInAndCorrectId_ReturnsNoContent()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var employeeUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.net")
        );

        var result = await client.DeleteAsync($"admin/users/employees/{employeeUser.UserId}");
        
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
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

        var employeeUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.net")
        );

        var result = await client.DeleteAsync($"admin/users/employees/{employeeUser.UserId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task DeleteEmployee_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var employeeUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.net")
        );

        var result = await client.DeleteAsync($"admin/users/employees/{employeeUser.UserId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task DeleteEmployee_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();
        
        var employeeUser = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.net")
        );

        var result = await client.DeleteAsync($"admin/users/employees/{employeeUser.UserId}");
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}