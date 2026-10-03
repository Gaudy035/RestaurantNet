using System.Net;
using System.Net.Http.Json;
using backend.Data.Entities;
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
        
        var response = await client.PostAsJsonAsync("/admin/auth/login", dto);
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
    public async Task GetClientById_WithLoggedInNonAdminEmployeeUser_ReturnsOk()
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
    public async Task GetClientById_WithLoggedInAdminEmployeeUser_ReturnsOk()
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
}