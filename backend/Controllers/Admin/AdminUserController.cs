using backend.DTOs.Users;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Admin;

[ApiController]
[Route("admin/user")]
public class AdminUserController: ControllerBase
{
    private readonly IUserService _userService;

    public AdminUserController(IUserService userService)
    {
        _userService = userService;
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployeeAccount([FromBody] CreateEmployeeDto dto)
    {
        var newEmployee = await _userService.CreateEmployee(dto);

        if (newEmployee == null)
        {
            return BadRequest(new { detail = "Failed to create employee account" });
        }

        return Ok(newEmployee);
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpPost("clients")]
    public async Task<IActionResult> CreateClientAccount([FromBody] CreateClientDto dto)
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
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees([FromQuery] string? param)
    {
        var foundEmployees = await _userService.FindEmployee(param);

        return Ok(foundEmployees);
    }
}