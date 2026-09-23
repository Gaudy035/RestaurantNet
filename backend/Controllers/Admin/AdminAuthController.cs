using System.Security.Claims;
using backend.DTOs.Auth;
using backend.Services;
using backend.Services.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers.Admin;

[ApiController]
[Route("admin/auth")]
public class AdminAuthController: ControllerBase
{
    private readonly IAuthService _authService;

    public AdminAuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private void CreateTokenCookies(string accessToken, string refreshToken)
    {
        Response.Cookies.Append("admin_access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Path = "/admin",
            Expires = DateTime.UtcNow.AddMinutes(15),
            SameSite = SameSiteMode.Lax
        });
        
        Response.Cookies.Append("admin_refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Path = "/admin/auth/refresh",
            Expires = DateTime.UtcNow.AddHours(12),
            SameSite = SameSiteMode.Lax
        });
    }

    private async Task RemoveTokenCookies()
    {
        var refreshTokenValue = Request.Cookies["admin_refresh_token"];
        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            await _authService.RevokeToken(refreshTokenValue);
        }
        Response.Cookies.Delete("admin_access_token", new CookieOptions{
            Path = "/admin"
        });
        Response.Cookies.Delete("admin_refresh_token", new CookieOptions{
            Path = "/admin/auth/refresh"
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.Login(dto, "Admin");

        if (!result.IsSuccess)
        {
            return result.ToActionResult(this);
        }

        var tokens = result.Data!;
        
        CreateTokenCookies(tokens.AccessToken, tokens.RefreshToken);
        
        return NoContent();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await RemoveTokenCookies();
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["admin_refresh_token"];
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized();
        }

        var result = await _authService.Refresh(refreshToken);

        if (!result.IsSuccess)
        {
            await RemoveTokenCookies();
            return result.ToActionResult(this);
        }

        var tokens = result.Data!;

        CreateTokenCookies(tokens.AccessToken, tokens.RefreshToken);

        return NoContent();
    }

    [Authorize(Roles = "Admin,Employee")]
    [HttpGet("me")]
    public async Task<IActionResult> MeAdmin()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdString == null || !int.TryParse(userIdString, out int userId))
        {
            return Unauthorized();
        }

        var result = await _authService.MeAdmin(userId);

        return result.ToActionResult(this);
    }
}