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
        var result = await _userService.CreateEmployee(dto);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees([FromQuery] string? param)
    {
        var result = await _userService.FindEmployee(param);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("employees/{employeeId:int}")]
    public async Task<IActionResult> GetEmployeeById([FromRoute] int employeeId)
    {
        var result = await _userService.FindEmployeeById(employeeId);

        return result.ToActionResult(this);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("employees/{employeeId:int}/locations")]
    public async Task<IActionResult> GetEmployeeLocations([FromRoute] int employeeId)
    {
        var result = await _userService.GetEmployeeLocations(employeeId);

        return result.ToActionResult(this);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("employees/{employeeId:int}")]
    public async Task<IActionResult> DeleteEmployee([FromRoute] int employeeId)
    {
        var currentAdminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _userService.DeleteEmployee(employeeId, currentAdminId);

        return result.ToActionResult(this);
    }
}