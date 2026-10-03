using System.Net;
using System.Net.Http.Json;
using backend.Data.Entities;
using backend.DTOs.Auth;
using backend.DTOs.Locations;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntegrationTests.Controllers.Admin;

public class AdminLocationControllerTests:IClassFixture<TestWebApplicationFactory>, IAsyncLifetime
{
    private readonly TestWebApplicationFactory _factory;

    public AdminLocationControllerTests(TestWebApplicationFactory factory)
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
                HandleCookies = true,
                AllowAutoRedirect = false
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
    public async Task CreateLocation_WithAdminEmployeeUserLoggedIn_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var dto = new LocationCreateDto
        {
            City = "TestCity",
            Address = "Test Address 42"
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/", dto);
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    [Fact]
    public async Task CreateLocation_WithAdminEmployeeUserLoggedInAndDuplicateData_ReturnsConflict()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var dto = new LocationCreateDto
        {
            City = "TestCity",
            Address = "Test Address 42"
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/", dto);
        
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateLocation_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client);

        var dto = new LocationCreateDto
        {
            Address = "Test Address",
            City = "Test City",
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/", dto);
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateLocation_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var dto = new LocationCreateDto
        {
            Address = "Test Address",
            City = "Test City",
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/", dto);
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task CreateLocation_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var dto = new LocationCreateDto
        {
            Address = "Test Address",
            City = "Test City",
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/", dto);
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
    
    [Fact]
    public async Task GetLocations_WithAdminEmployeeUserLoggedIn_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);
        
        var result = await client.GetAsync("/admin/locations/");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    
    [Fact]
    public async Task GetLocations_WithNonAdminEmployeeUserLoggedIn_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client);
        
        var result = await client.GetAsync("/admin/locations/");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    
    [Fact]
    public async Task GetLocations_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);
        
        var result = await client.GetAsync("/admin/locations/");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task GetLocations_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();
        
        var result = await client.GetAsync("/admin/locations/");
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
    
    [Fact]
    public async Task FindLocationById_WithAdminEmployeeUserLoggedInAndCorrectId_ReturnsOk()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );
        
        var result = await client.GetAsync($"/admin/locations/{location.LocationId}");
        
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
    
    [Fact]
    public async Task FindLocationById_WithAdminEmployeeUserLoggedInAndIncorrectId_ReturnsNotFound()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);
        
        var result = await client.GetAsync("/admin/locations/42");
        
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
    
    [Fact]
    public async Task FindLocationById_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );
        
        var result = await client.GetAsync($"/admin/locations/{location.LocationId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task FindLocationById_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );
        
        var result = await client.GetAsync($"/admin/locations/{location.LocationId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task FindLocationById_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();
        
        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );
        
        var result = await client.GetAsync($"/admin/locations/{location.LocationId}");
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
    
    [Fact]
    public async Task DeleteLocation_WithAdminEmployeeUserLoggedInAndCorrectId_ReturnsNoContent()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );
        
        var result = await client.DeleteAsync($"/admin/locations/{location.LocationId}");
        
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
    
    [Fact]
    public async Task DeleteLocation_WithAdminEmployeeUserLoggedInAndIncorrectId_ReturnsNotFound()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);
        
        var result = await client.DeleteAsync($"/admin/locations/42");
        
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
    
    [Fact]
    public async Task DeleteLocation_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );
        
        var result = await client.DeleteAsync($"/admin/locations/{location.LocationId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task DeleteLocation_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );
        
        var result = await client.DeleteAsync($"/admin/locations/{location.LocationId}");
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task DeleteLocation_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );
        
        var result = await client.DeleteAsync($"/admin/locations/{location.LocationId}");
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task AssignEmployee_WithAdminEmployeeUserLoggedInAndCorrectIds_ReturnsNoContent()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var dto = new LocationAssignEmployeeDto
        {
            LocationId = location.LocationId,
            UserId = employee.UserId,
            Position = Position.Cashier
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/employees", dto);
        
        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
    }
    
    [Fact]
    public async Task AssignEmployee_WithAdminEmployeeUserLoggedInAndIncorrectLocationId_ReturnsNotFound()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );

        var dto = new LocationAssignEmployeeDto
        {
            LocationId = 42,
            UserId = employee.UserId,
            Position = Position.Cashier
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/employees", dto);
        
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
    
    [Fact]
    public async Task AssignEmployee_WithAdminEmployeeUserLoggedInAndIncorrectUserId_ReturnsNotFound()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var dto = new LocationAssignEmployeeDto
        {
            LocationId = location.LocationId,
            UserId = 42,
            Position = Position.Cashier
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/employees", dto);
        
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }
    
    [Fact]
    public async Task AssignEmployee_WithAdminEmployeeUserLoggedInAndDuplicateData_ReturnsConflict()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client, true);

        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(
                context,
                userId: employee.UserId,
                locationId: location.LocationId,
                position: Position.Cashier
                )
        );

        var dto = new LocationAssignEmployeeDto
        {
            LocationId = location.LocationId,
            UserId = employee.UserId,
            Position = Position.Cashier
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/employees", dto);
        
        Assert.Equal(HttpStatusCode.Conflict, result.StatusCode);
    }
    
    [Fact]
    public async Task AssignEmployee_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await EmployeeLogin(client);

        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var dto = new LocationAssignEmployeeDto
        {
            LocationId = location.LocationId,
            UserId = employee.UserId,
            Position = Position.Cashier
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/employees", dto);
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task AssignEmployee_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();
        
        await ClientLogin(client);

        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var dto = new LocationAssignEmployeeDto
        {
            LocationId = location.LocationId,
            UserId = employee.UserId,
            Position = Position.Cashier
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/employees", dto);
        
        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }
    
    [Fact]
    public async Task AssignEmployee_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();
        
        var employee = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );

        var location = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedLocation(context)
        );

        var dto = new LocationAssignEmployeeDto
        {
            LocationId = location.LocationId,
            UserId = employee.UserId,
            Position = Position.Cashier
        };
        
        var result = await client.PostAsJsonAsync("/admin/locations/employees", dto);
        
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}