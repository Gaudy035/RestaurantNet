using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Locations;
using backend.Services;
using UnitTests.Helpers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Services;

public class LocationServiceTests: IDisposable
{
    private readonly AppDbContext _context;
    private readonly SqliteConnection _connection;
    private readonly LocationService _locationService;

    public LocationServiceTests()
    {
        (_context, _connection) = TestDbContextFactory.Create();
        _locationService = new LocationService(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
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

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.NotEqual(0, result.Data.LocationId);
        Assert.Equal("TestCity", result.Data.City);
        Assert.Equal("TestAddress", result.Data.Address);
    }

    [Fact]
    public async Task GetLocations_WhenNoLocationsExist_ReturnEmpty()
    {
        var result = await _locationService.GetLocations(null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetLocations_WithNoParam_ReturnsAllLocations()
    {
        var location1 = await LocationSeeder.SeedLocation(_context);
        var location2 = await LocationSeeder.SeedLocation(
            _context,
            city: "City2",
            address: "Address2"
            );

        var result = await _locationService.GetLocations(null);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        Assert.Contains(result.Data, l => l.City == "City1" && l.Address == "Address1");
        Assert.Contains(result.Data, l => l.City == "City2" && l.Address == "Address2");
    }

    [Fact]
    public async Task GetLocation_WithCorrectParam_ReturnsAllMatches()
    {
        var location1 = await LocationSeeder.SeedLocation(_context);
        var location2 = await LocationSeeder.SeedLocation(
            _context,
            city: "City2",
            address: "Address2"
            );
        var location3 = await LocationSeeder.SeedLocation(
            _context,
            city: "City3",
            address: "Address2"
            );

        var result = await _locationService.GetLocations("ess2");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count());
        Assert.Contains(result.Data, l => l.City == "City2" && l.Address == "Address2");
        Assert.Contains(result.Data, l => l.City == "City3" && l.Address == "Address2");
        Assert.DoesNotContain(result.Data, l => l.City == "City1" && l.Address == "Address1");
    }

    [Fact]
    public async Task GetLocation_WithNoMatches_ReturnEmpty()
    {
        await LocationSeeder.SeedLocation(_context);

        var result = await _locationService.GetLocations("NonExistantAddress");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task FindLocationById_WithCorrectId_ReturnsCorrectLocationData()
    {
        var location = await LocationSeeder.SeedLocation(_context);

        var result = await _locationService.FindLocationById(location.LocationId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(location.LocationId, result.Data.LocationId);
        Assert.Equal(location.City, result.Data.City);
        Assert.Equal(location.Address, result.Data.Address);
    }

    [Fact]
    public async Task FindLocationById_WithIncorrectId_ReturnsLocationNotFoundError()
    {
        await LocationSeeder.SeedLocation(_context);

        var result = await _locationService.FindLocationById(42);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Data);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
    }

    [Fact]
    public async Task DeleteLocation_WithCorrectId_DeletesAndReturnsSuccess()
    {
        var location = await LocationSeeder.SeedLocation(_context);

        var before = await _context.Locations.CountAsync();
        var result = await _locationService.DeleteLocation(location.LocationId);
        var after = await _context.Locations.CountAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, before);
        Assert.Equal(0, after);
    }

    [Fact]
    public async Task DeleteLocation_WithIncorrectId_DoesntRemoveAndReturnsLocationNotFoundError()
    {
        await LocationSeeder.SeedLocation(_context);

        var before = await _context.Locations.CountAsync();
        var result = await _locationService.DeleteLocation(42);
        var after = await _context.Locations.CountAsync();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task DeleteLocation_WithCorrectId_DeletesEmployeeAsignmentsButKeepsEmployees()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);
        await AssignmentSeeder.SeedAssignment(_context, employee.UserId, location.LocationId, Position.Server);

        var employeesBefore = await _context.Employees.CountAsync();
        var assignmentsBefore = await _context.LocationEmployees.CountAsync();
        var locationsBefore = await _context.Locations.CountAsync();

        await _locationService.DeleteLocation(location.LocationId);

        var employeesAfter = await _context.Employees.CountAsync();
        var assignmentsAfter = await _context.LocationEmployees.CountAsync();
        var locationsAfter = await _context.Locations.CountAsync();

        Assert.Equal(1, employeesBefore);
        Assert.Equal(1, locationsBefore);
        Assert.Equal(1, assignmentsBefore);

        Assert.Equal(employeesBefore, employeesAfter);
        Assert.Equal(0, locationsAfter);
        Assert.Equal(0, assignmentsAfter);
    }
    
    [Fact]
    public async Task AssignEmployee_WithCorrectData_AssignsEmployeeAndReturnsSuccess()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);

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

        Assert.True(result.IsSuccess);
        Assert.Equal(1, assignedEmployees);
        
    }

    [Fact]
    public async Task AssignEmployee_WithClientId_DoesntAssignAndReturnsEmployeeNotFoundError()
    {
        var client = await ClientUserSeeder.SeedClientUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);

        var dto = new LocationAssignEmployeeDto
        {
            UserId = client.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var result = await _locationService.AssignEmployee(dto);
        var assignedEmployees = await _context.LocationEmployees.CountAsync();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
        Assert.Equal(0, assignedEmployees);
    }

    [Fact]
    public async Task AssignEmployee_WithNonExistentUser_DoesntAssignAndReturnsEmployeeNotFoundError()
    {
        var location = await LocationSeeder.SeedLocation(_context);

        var dto = new LocationAssignEmployeeDto
        {
            UserId = 42,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var result = await _locationService.AssignEmployee(dto);
        var assignedEmployees = await _context.LocationEmployees.CountAsync();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
        Assert.Equal(0, assignedEmployees);
    }

    [Fact]
    public async Task AssignEmployee_WithNonExistentLocation_DoesntAssignAndReturnsLocationNotFoundError()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = 42,
            Position = Position.Cashier
        };

        var result = await _locationService.AssignEmployee(dto);
        var assignedEmployees = await _context.LocationEmployees.CountAsync();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
        Assert.Equal(0, assignedEmployees);
    }

    [Fact]
    public async Task AssignEmployee_WithDuplicateData_DoesntAssignAndReturnsAlreadyAssignedError()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var result1 = await _locationService.AssignEmployee(dto);
        var result2 = await _locationService.AssignEmployee(dto);
        var assignedEmployees = await _context.LocationEmployees.CountAsync();

        Assert.True(result1.IsSuccess);
        Assert.False(result2.IsSuccess);
        Assert.NotNull(result2.Error);
        Assert.Equal(409, result2.Error.StatusCode);
        Assert.Equal(1, assignedEmployees);
    }

    [Fact]
    public async Task AssignEmployee_WithSameIdsAndDifferentPosition_AssignsAndReturnsTrue()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);

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

        Assert.True(result1.IsSuccess);
        Assert.True(result2.IsSuccess);
        Assert.Equal(2, assignedEmployees);
    }

    [Fact]
    public async Task GetAssignedEmployees_WithMatchingEmployees_ReturnsAllMatches()
    {
        var employee1 = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var employee2 = await EmployeeUserSeeder.SeedEmployeeUser(
            _context,
            firstName: "Jane",
            lastName: "Doe",
            email: "jane@example.net"
            );
        var location = await LocationSeeder.SeedLocation(_context);
        await AssignmentSeeder.SeedAssignment(_context, employee1.UserId, location.LocationId, Position.Chef);
        await AssignmentSeeder.SeedAssignment(_context, employee2.UserId, location.LocationId, Position.Cashier);

        var assignmentCount = await _context.LocationEmployees.CountAsync();
        var result = await _locationService.GetAssignedEmployees(location.LocationId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, assignmentCount);
        Assert.Equal(assignmentCount, result.Data.Count());
        Assert.Contains(result.Data, x =>
            x.UserId == employee1.UserId &&
            x.FirstName == employee1.User.FirstName &&
            x.LastName == employee1.User.LastName &&
            x.LocationId == location.LocationId &&
            x.Position == Position.Chef
        );
        Assert.Contains(result.Data, x =>
            x.UserId == employee2.UserId &&
            x.FirstName == employee2.User.FirstName &&
            x.LastName == employee2.User.LastName &&
            x.LocationId == location.LocationId &&
            x.Position == Position.Cashier
        );
    }

    [Fact]
    public async Task GetAssignedEmployees_WithoutMatches_ReturnsEmptyList()
    {
        var location = await LocationSeeder.SeedLocation(_context);

        var result = await _locationService.GetAssignedEmployees(location.LocationId);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task GetAssignedEmployees_WithMatches_ReturnsOnlyThoseAtRightLocation()
    {
        var location1 = await LocationSeeder.SeedLocation(_context);
        var employee1 = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        await AssignmentSeeder.SeedAssignment(_context, employee1.UserId, location1.LocationId, Position.Server);

        var location2 = await LocationSeeder.SeedLocation(
            _context,
            city: "City2",
            address: "Address2"
            );
        var employee2 = await EmployeeUserSeeder.SeedEmployeeUser(
            _context,
            firstName: "Jane",
            lastName: "Doe",
            email: "jane@example.net"
            );
        await AssignmentSeeder.SeedAssignment(_context, employee2.UserId, location2.LocationId, Position.Server);

        var countAssigned = await _context.LocationEmployees.CountAsync();
        var result = await _locationService.GetAssignedEmployees(location1.LocationId);

        Assert.Equal(2, countAssigned);
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data);
    }

    [Fact]
    public async Task UnassignEmployee_AfterRemoving_DoesntRemoveLocationOrEmployee()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        await _locationService.AssignEmployee(dto);
        await _locationService.UnassignEmployee(dto);

        var employeeExists = await _context.Employees
            .AsNoTracking()
            .Where(e => e.UserId == employee.UserId)
            .AnyAsync();

        var locationExists = await _context.Locations
            .AsNoTracking()
            .Where(l => l.LocationId == location.LocationId)
            .AnyAsync();

        Assert.True(employeeExists);
        Assert.True(locationExists);
    }

    [Fact]
    public async Task UnassignEmployee_WithCorrectData_RemovesAssignmentAndReturnsSuccess()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);

        var dto = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        await _locationService.AssignEmployee(dto);
        var before = await _context.LocationEmployees.CountAsync();

        var result = await _locationService.UnassignEmployee(dto);
        var after = await _context.LocationEmployees.CountAsync();

        Assert.True(result.IsSuccess);
        Assert.Equal(1, before);
        Assert.Equal(0, after);
    }

    [Fact]
    public async Task UnassignEmployee_WithIncorrectLocationId_DoesntRemoveAndReturnsAssignmentNotFoundError()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);

        var dto1 = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var dto2 = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = 42,
            Position = Position.Cashier
        };

        await _locationService.AssignEmployee(dto1);
        var before = await _context.LocationEmployees.CountAsync();

        var result = await _locationService.UnassignEmployee(dto2);
        var after = await _context.LocationEmployees.CountAsync();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task UnassignEmployee_WithIncorrectUserId_DoesntRemoveAndReturnsAssignmentNotFoundError()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);

        var dto1 = new LocationAssignEmployeeDto
        {
            UserId = employee.UserId,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        var dto2 = new LocationAssignEmployeeDto
        {
            UserId = 42,
            LocationId = location.LocationId,
            Position = Position.Cashier
        };

        await _locationService.AssignEmployee(dto1);
        var before = await _context.LocationEmployees.CountAsync();

        var result = await _locationService.UnassignEmployee(dto2);
        var after = await _context.LocationEmployees.CountAsync();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task UnassignEmployee_WithIncorrectPosition_DoesntRemoveAndReturnsAssignmentNotFoundError()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);

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
            Position = Position.Driver
        };

        await _locationService.AssignEmployee(dto1);
        var before = await _context.LocationEmployees.CountAsync();

        var result = await _locationService.UnassignEmployee(dto2);
        var after = await _context.LocationEmployees.CountAsync();

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(404, result.Error.StatusCode);
        Assert.Equal(before, after);
    }

    [Fact]
    public async Task UnassignEmployee_WithMultipleIdMatches_OnlyDeletesWithMatchingPosition()
    {
        var employee = await EmployeeUserSeeder.SeedEmployeeUser(_context);
        var location = await LocationSeeder.SeedLocation(_context);

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
            Position = Position.Driver
        };

        await _locationService.AssignEmployee(dto1);
        await _locationService.AssignEmployee(dto2);

        var before = await _context.LocationEmployees.CountAsync();
        await _locationService.UnassignEmployee(dto1);

        var after = await _context.LocationEmployees.CountAsync();

        Assert.Equal(2, before);
        Assert.Equal(1, after);
    }
}   