using System.Net;
using System.Net.Http.Json;
using backend.Data.Entities;
using backend.DTOs.Auth;
using backend.DTOs.Locations;
using IntegrationTests.Helpers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

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

    private async Task<Location> SeedLocation(string city = "TestCity", string address = "Test Address 42") =>
        await DatabaseHelper.ExecuteAsync(_factory, context => LocationSeeder.SeedLocation(context, city, address));

    private async Task<Employee> SeedSecondEmployee() =>
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "secondemployee@example.com")
        );

    [Fact]
    public async Task CreateLocation_WithAdminEmployeeUserLoggedIn_CreatesLocation()
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

        var location = await result.ReadJsonAsync<LocationResponseDto>();
        Assert.True(location.LocationId > 0);
        Assert.Equal("TestCity", location.City);
        Assert.Equal("Test Address 42", location.Address);

        var persisted = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => context.Locations.AsNoTracking()
                .FirstOrDefaultAsync(l => l.City == "TestCity" && l.Address == "Test Address 42")
        );
        Assert.NotNull(persisted);
    }

    [Fact]
    public async Task CreateLocation_WithAdminEmployeeUserLoggedInAndDuplicateData_ReturnsConflict()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        await SeedLocation();

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
    public async Task GetLocations_WithAdminEmployeeUserLoggedIn_ReturnsAllLocations()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        await SeedLocation("City2", "Address2");
        await SeedLocation("City1", "Address1");

        var result = await client.GetAsync("/admin/locations/");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var locations = await result.ReadJsonAsync<List<LocationResponseDto>>();
        Assert.Equal(
            ["City1 Address1", "City2 Address2"],
            locations.Select(l => $"{l.City} {l.Address}"));
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
    public async Task GetLocations_WithSearchParam_ReturnsOnlyMatches()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        await SeedLocation("City1", "Address1");
        await SeedLocation("City2", "Address2");
        await SeedLocation("City3", "Address2");

        var result = await client.GetAsync("/admin/locations/?param=ess2");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var locations = await result.ReadJsonAsync<List<LocationResponseDto>>();
        Assert.Equal(2, locations.Count);
        Assert.Contains(locations, l => l.City == "City2" && l.Address == "Address2");
        Assert.Contains(locations, l => l.City == "City3" && l.Address == "Address2");
        Assert.DoesNotContain(locations, l => l.City == "City1");
    }

    [Fact]
    public async Task GetLocations_WithSearchParamAndNoMatches_ReturnsEmpty()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        await SeedLocation();

        var result = await client.GetAsync("/admin/locations/?param=DoesNotExist");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var locations = await result.ReadJsonAsync<List<LocationResponseDto>>();
        Assert.Empty(locations);
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
    public async Task FindLocationById_WithAdminEmployeeUserLoggedInAndCorrectId_ReturnsLocation()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var location = await SeedLocation("City7", "Address7");

        var result = await client.GetAsync($"/admin/locations/{location.LocationId}");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var body = await result.ReadJsonAsync<LocationResponseDto>();
        Assert.Equal(location.LocationId, body.LocationId);
        Assert.Equal("City7", body.City);
        Assert.Equal("Address7", body.Address);
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

        var location = await SeedLocation();

        var result = await client.GetAsync($"/admin/locations/{location.LocationId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task FindLocationById_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var location = await SeedLocation();

        var result = await client.GetAsync($"/admin/locations/{location.LocationId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task FindLocationById_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var location = await SeedLocation();

        var result = await client.GetAsync($"/admin/locations/{location.LocationId}");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task DeleteLocation_WithAdminEmployeeUserLoggedInAndCorrectId_RemovesLocationAndAssignmentsButKeepsEmployee()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employee.UserId, location.LocationId, Position.Server)
        );

        var result = await client.DeleteAsync($"/admin/locations/{location.LocationId}");

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        var state = await DatabaseHelper.ExecuteAsync(_factory, async context => new
        {
            LocationExists = await context.Locations.AnyAsync(l => l.LocationId == location.LocationId),
            AssignmentCount = await context.LocationEmployees.CountAsync(),
            EmployeeExists = await context.Employees.AnyAsync(e => e.UserId == employee.UserId)
        });

        Assert.False(state.LocationExists);
        Assert.Equal(0, state.AssignmentCount);
        Assert.True(state.EmployeeExists);
    }

    [Fact]
    public async Task DeleteLocation_WithAdminEmployeeUserLoggedInAndIncorrectId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var result = await client.DeleteAsync("/admin/locations/42");

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task DeleteLocation_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var location = await SeedLocation();

        var result = await client.DeleteAsync($"/admin/locations/{location.LocationId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task DeleteLocation_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var location = await SeedLocation();

        var result = await client.DeleteAsync($"/admin/locations/{location.LocationId}");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task DeleteLocation_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var location = await SeedLocation();

        var result = await client.DeleteAsync($"/admin/locations/{location.LocationId}");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task AssignEmployee_WithAdminEmployeeUserLoggedInAndCorrectIds_AssignsEmployee()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

        var dto = new LocationAssignEmployeeDto
        {
            LocationId = location.LocationId,
            UserId = employee.UserId,
            Position = Position.Cashier
        };

        var result = await client.PostAsJsonAsync("/admin/locations/employees", dto);

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        var assigned = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => context.LocationEmployees.AsNoTracking()
                .AnyAsync(le => le.UserId == employee.UserId
                    && le.LocationId == location.LocationId
                    && le.Position == Position.Cashier)
        );
        Assert.True(assigned);
    }

    [Fact]
    public async Task AssignEmployee_WithAdminEmployeeUserLoggedInAndIncorrectLocationId_ReturnsNotFound()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employee = await SeedSecondEmployee();

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

        var location = await SeedLocation();

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

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employee.UserId, location.LocationId, Position.Cashier)
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

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

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

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

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

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

        var dto = new LocationAssignEmployeeDto
        {
            LocationId = location.LocationId,
            UserId = employee.UserId,
            Position = Position.Cashier
        };

        var result = await client.PostAsJsonAsync("/admin/locations/employees", dto);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task GetAssignedEmployees_WithAdminEmployeeUserLoggedIn_ReturnsAssignedEmployees()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employee1 = await SeedSecondEmployee();
        var employee2 = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "thirdemployee@example.com", firstName: "Rick", lastName: "Sanchez")
        );
        var location = await SeedLocation();
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employee1.UserId, location.LocationId, Position.Cashier)
        );
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employee2.UserId, location.LocationId, Position.Chef)
        );

        var result = await client.GetAsync($"/admin/locations/{location.LocationId}/employees");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var employees = await result.ReadJsonAsync<List<LocationEmployeesResponseDto>>();
        Assert.Equal(2, employees.Count);

        Assert.Contains(employees, e =>
            e.UserId == employee2.UserId &&
            e.FirstName == "Rick" &&
            e.LastName == "Sanchez" &&
            e.LocationId == location.LocationId &&
            e.Position == Position.Chef);

        Assert.Contains(employees, e =>
            e.UserId == employee1.UserId &&
            e.FirstName == "Jane" &&
            e.LastName == "Doe" &&
            e.LocationId == location.LocationId &&
            e.Position == Position.Cashier);
    }

    [Fact]
    public async Task GetAssignedEmployees_WithSamePosition_OrdersByLastName()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employeeZulu = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "zulu@example.com", firstName: "Zed", lastName: "Zulu")
        );
        var employeeAlpha = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => UserSeeder.SeedEmployeeUser(context, email: "alpha@example.com", firstName: "Amy", lastName: "Alpha")
        );
        var location = await SeedLocation();
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employeeZulu.UserId, location.LocationId, Position.Server)
        );
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employeeAlpha.UserId, location.LocationId, Position.Server)
        );

        var result = await client.GetAsync($"/admin/locations/{location.LocationId}/employees");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var employees = await result.ReadJsonAsync<List<LocationEmployeesResponseDto>>();
        Assert.Equal(new[] { "Alpha", "Zulu" }, employees.Select(e => e.LastName));
    }

    [Fact]
    public async Task GetAssignedEmployees_WithNoAssignments_ReturnsEmpty()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var location = await SeedLocation();

        var result = await client.GetAsync($"/admin/locations/{location.LocationId}/employees");

        Assert.Equal(HttpStatusCode.OK, result.StatusCode);

        var employees = await result.ReadJsonAsync<List<LocationEmployeesResponseDto>>();
        Assert.Empty(employees);
    }

    [Fact]
    public async Task GetAssignedEmployees_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var location = await SeedLocation();

        var result = await client.GetAsync($"/admin/locations/{location.LocationId}/employees");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetAssignedEmployees_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var location = await SeedLocation();

        var result = await client.GetAsync($"/admin/locations/{location.LocationId}/employees");

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task GetAssignedEmployees_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var location = await SeedLocation();

        var result = await client.GetAsync($"/admin/locations/{location.LocationId}/employees");

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }

    [Fact]
    public async Task UnassignEmployee_WithAdminEmployeeUserLoggedInAndCorrectIds_RemovesAssignmentButKeepsLocationAndEmployee()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

        await DatabaseHelper.ExecuteAsync(
            _factory,   
            context => LocationSeeder.SeedAssignment(context, employee.UserId, location.LocationId, Position.Cashier)
        );

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/admin/locations/employees")
        {
            Content = JsonContent.Create(dto)
        };

        var result = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        var state = await DatabaseHelper.ExecuteAsync(_factory, async context => new
        {
            AssignmentCount = await context.LocationEmployees.CountAsync(),
            LocationExists = await context.Locations.AnyAsync(l => l.LocationId == location.LocationId),
            EmployeeExists = await context.Employees.AnyAsync(e => e.UserId == employee.UserId)
        });

        Assert.Equal(0, state.AssignmentCount);
        Assert.True(state.LocationExists);
        Assert.True(state.EmployeeExists);
    }

    [Fact]
    public async Task UnassignEmployee_WithAdminEmployeeUserLoggedInAndMultiplePositions_RemovesOnlyMatchingPosition()
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employee.UserId, location.LocationId, Position.Cashier)
        );
        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employee.UserId, location.LocationId, Position.Driver)
        );

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/admin/locations/employees")
        {
            Content = JsonContent.Create(dto)
        };

        var result = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);

        var remaining = await DatabaseHelper.ExecuteAsync(
            _factory,
            context => context.LocationEmployees.AsNoTracking().ToListAsync()
        );

        Assert.Single(remaining);
        Assert.Equal(Position.Driver, remaining[0].Position);
    }

    [Theory]
    [InlineData(0, 1, Position.Cashier)] // wrong user
    [InlineData(1, 0, Position.Cashier)] // wrong location
    [InlineData(1, 1, Position.Driver)]  // wrong position
    public async Task UnassignEmployee_WithIncorrectData_ReturnsNotFound(int userOffset, int locationOffset, Position position)
    {
        var client = CreateClient();

        await EmployeeLogin(client, true);

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

        await DatabaseHelper.ExecuteAsync(
            _factory,
            context => LocationSeeder.SeedAssignment(context, employee.UserId, location.LocationId, Position.Cashier)
        );

        var dto = new LocationAssignEmployeeDto
        {
            UserId = userOffset == 0 ? 42 : employee.UserId,
            LocationId = locationOffset == 0 ? 42 : location.LocationId,
            Position = position
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/admin/locations/employees")
        {
            Content = JsonContent.Create(dto)
        };

        var result = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
    }

    [Fact]
    public async Task UnassignEmployee_WithNonAdminEmployeeUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await EmployeeLogin(client);

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/admin/locations/employees")
        {
            Content = JsonContent.Create(dto)
        };

        var result = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task UnassignEmployee_WithClientUserLoggedIn_ReturnsForbidden()
    {
        var client = CreateClient();

        await ClientLogin(client);

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/admin/locations/employees")
        {
            Content = JsonContent.Create(dto)
        };

        var result = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, result.StatusCode);
    }

    [Fact]
    public async Task UnassignEmployee_WithNoUserLoggedIn_ReturnsUnauthorized()
    {
        var client = CreateClient();

        var employee = await SeedSecondEmployee();
        var location = await SeedLocation();

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var request = new HttpRequestMessage(HttpMethod.Delete, "/admin/locations/employees")
        {
            Content = JsonContent.Create(dto)
        };

        var result = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}
