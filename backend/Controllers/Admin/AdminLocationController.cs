using backend.DTOs.Locations;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("admin/locations")]
public class AdminLocationsController: ControllerBase
{
    private readonly ILocationService _locationService;

    public AdminLocationsController(ILocationService locationService)
    {
        _locationService = locationService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost()]
    public async Task<IActionResult> CreateLocation(LocationCreateDto dto)
    {
        var newLocation = await _locationService.CreateLocation(dto);

        if (newLocation == null)
        {
            return StatusCode(500);
        }

        return Ok(newLocation);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet()]
    public async Task<IActionResult> GetLocations([FromQuery] string? param)
    {
        var foundLocations = await _locationService.GetLocations(param);

        return Ok(foundLocations);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{locationId:int}")]
    public async Task<IActionResult> FindLocation([FromRoute] int locationId)
    {
        var location = await _locationService.FindLocation(locationId);

        if (location == null)
        {
            return NotFound(new { detail = "Location not found" });
        }

        return Ok(location);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{locationId:int}")]
    public async Task<IActionResult> DeleteLocation([FromRoute] int locationId)
    {
        var success = await _locationService.DeleteLocation(locationId);

        if (!success)
        {
            return NotFound(new { detail = $"Location with id {locationId} not found" });
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("employees")]
    public async Task<IActionResult> AssignEmployee(LocationAssignEmployeeDto dto)
    {
        var success = await _locationService.AssignEmployee(dto);

        if (!success)
        {
            return BadRequest(new { detail = "Assignment failed" });
        }

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("employees/{locationId:int}")]
    public async Task<IActionResult> GetAssignedEmployees([FromRoute] int locationId)
    {
        var employees = await _locationService.GetAssignedEmployees(locationId);

        return Ok(employees);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("employees")]
    public async Task<IActionResult> UnassignEmployee(LocationAssignEmployeeDto dto)
    {
        var success = await _locationService.UnassignEmployee(dto);

        if (!success)
        {
            return BadRequest(new { detail = "Unassignment failed" });
        }

        return NoContent();
    }
}