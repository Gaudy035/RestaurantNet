using backend.DTOs.Locations;
using backend.Services;
using backend.Services.Errors;
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
        var result = await _locationService.CreateLocation(dto);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet()]
    public async Task<IActionResult> GetLocations([FromQuery] string? param)
    {
        var result = await _locationService.GetLocations(param);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{locationId:int}")]
    public async Task<IActionResult> FindLocationById([FromRoute] int locationId)
    {
        var result = await _locationService.FindLocationById(locationId);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{locationId:int}")]
    public async Task<IActionResult> DeleteLocation([FromRoute] int locationId)
    {
        var result = await _locationService.DeleteLocation(locationId);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("employees")]
    public async Task<IActionResult> AssignEmployee(LocationAssignEmployeeDto dto)
    {
        var result = await _locationService.AssignEmployee(dto);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{locationId:int}/employees")]
    public async Task<IActionResult> GetAssignedEmployees([FromRoute] int locationId)
    {
        var result = await _locationService.GetAssignedEmployees(locationId);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("employees")]
    public async Task<IActionResult> UnassignEmployee(LocationAssignEmployeeDto dto)
    {
        var result = await _locationService.UnassignEmployee(dto);

        return result.ToActionResult(this);
    }
}