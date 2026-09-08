using System.Security.Claims;
using backend.DTOs.Auth;
using backend.DTOs.Users;
using backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("auth")]
public class AuthController: ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;    
    }

    private void CreateTokenCookies(string accessToken, string refreshToken)
    {
        Response.Cookies.Append("client_access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Path = "/",
            Expires = DateTime.UtcNow.AddMinutes(15),
            SameSite = SameSiteMode.Lax
        });

        Response.Cookies.Append("client_refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Path = "/auth/refresh",
            Expires = DateTime.UtcNow.AddDays(7),
            SameSite = SameSiteMode.Lax
        });
    }

    private async Task RemoveTokenCookies()
    {
        var refreshTokenValue = Request.Cookies["client_refresh_token"];
        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            await _authService.RevokeToken(refreshTokenValue);
        }
        Response.Cookies.Delete("client_access_token", new CookieOptions
        {
            Path = "/"
        });
        Response.Cookies.Delete("client_refresh_token", new CookieOptions
        {
            Path = "/auth/refresh"
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var loginResponse = await _authService.Login(dto, "Client");

        if (loginResponse == null)
        {
            return Unauthorized();
        }

        CreateTokenCookies(loginResponse.AccessToken, loginResponse.RefreshToken);
        return Ok(new { message = "Logged in successfully" });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await RemoveTokenCookies();
        return Ok(new { message = "Logged out successfully" });
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["client_refresh_token"];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized();
        }

        var newTokens = await _authService.Refresh(refreshToken);

        if (newTokens == null)
        {
            await RemoveTokenCookies();
            return Unauthorized();
        }

        CreateTokenCookies(newTokens.AccessToken, newTokens.RefreshToken);

        return Ok(new { message = "Tokens refreshed" });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] ClientCreateDto dto)
    {
        var createClientResponse = await _userService.CreateClient(dto);

        if (createClientResponse == null)
        {
            return BadRequest();
        }

        var loginDto = new LoginDto
        {
            Email = dto.Email,
            Password = dto.Password
        };

        var loginResponse = await _authService.Login(loginDto, "Client");

        CreateTokenCookies(loginResponse!.AccessToken, loginResponse!.RefreshToken);

        return Ok(new { message = "Registered successfully" });
    }

    [Authorize(Roles = "Client")]
    [HttpGet("me")]
    public async Task<IActionResult> MeClient()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized();
        }

        var clientData = await _authService.MeClient(userId);

        if (clientData == null)
        {
            return NotFound(new { message = "Client not found" });
        }

        return Ok(clientData);
    }
}