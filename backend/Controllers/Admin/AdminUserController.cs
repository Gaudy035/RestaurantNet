using System.Security.Claims;
using backend.DTOs.Users;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Services.Errors;

namespace backend.Controllers.Admin;

[ApiController]
[Route("admin/users")]
public class AdminUserController: ControllerBase
{
    private readonly IUserService _userService;

    public AdminUserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpPost("clients")]
    public async Task<IActionResult> CreateClientAccount([FromBody] ClientCreateDto dto)
    {
        var result = await _userService.CreateClient(dto);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("clients")]
    public async Task<IActionResult> GetClients([FromQuery] string? param)
    {
        var result = await _userService.FindClient(param);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("clients/{clientId:int}")]
    public async Task<IActionResult> GetClientById([FromRoute] int clientId)
    {
        var result = await _userService.FindClientById(clientId);

        return result.ToActionResult(this);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("clients/{clientId:int}")]
    public async Task<IActionResult> DeleteClient([FromRoute] int clientId)
    {
        var result = await _userService.DeleteClient(clientId);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployeeAccount([FromBody] EmployeeCreateDto dto)
    {
        var newEmployee = await _userService.CreateEmployee(dto);

        if (newEmployee == null)
        {
            return BadRequest(new { detail = "Failed to create employee account" });
        }

        return Ok(newEmployee);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees([FromQuery] string? param)
    {
        var foundEmployees = await _userService.FindEmployee(param);

        return Ok(foundEmployees);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("employees/{employeeId:int}")]
    public async Task<IActionResult> GetEmployeeById([FromRoute] int employeeId)
    {
        var employee = await _userService.FindEmployeeById(employeeId);

        if (employee == null)
        {
            return NotFound(new { detail = $"Employee with ID {employeeId} not found" });
        }

        return Ok(employee);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("employees/{employeeId:int}/locations")]
    public async Task<IActionResult> GetEmployeeLocations([FromRoute] int employeeId)
    {
        var locations = await _userService.GetEmployeeLocations(employeeId);

        return Ok(locations);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("employees/{employeeId:int}")]
    public async Task<IActionResult> DeleteEmployee([FromRoute] int employeeId)
    {
        var activeAdminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        if (activeAdminId == employeeId)
        {
            return BadRequest(new { detail = "You cannot delete your own account" });
        }

        var success = await _userService.DeleteEmployee(employeeId);

        if (!success)
        {
            return NotFound(new { detail = $"Employee with ID {employeeId} doesn't exist." });
        }

        return NoContent();
    }
}