using System.Security.Claims;
using backend.DTOs.Users;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        var newClient = await _userService.CreateClient(dto);

        if (newClient == null)
        {
            return BadRequest(new { detail = "Failed to create client account" });
        }

        return Ok(newClient);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("clients")]
    public async Task<IActionResult> GetClients([FromQuery] string? param)
    {
        var foundClients = await _userService.FindClient(param);

        return Ok(foundClients);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("clients/{clientId:int}")]
    public async Task<IActionResult> DeleteClient([FromRoute] int clientId)
    {
        var success = await _userService.DeleteClient(clientId);

        if (!success)
        {
            return NotFound(new { detail = $"Client with ID {clientId} doesn't exist." });
        }

        return NoContent();
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