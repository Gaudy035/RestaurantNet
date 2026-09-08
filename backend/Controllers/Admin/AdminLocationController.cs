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

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet()]
    public async Task<IActionResult> GetLocations()
    {
        var foundLocations = await _locationService.GetLocations();

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

}