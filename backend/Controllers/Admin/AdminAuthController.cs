using backend.DTOs.Auth;
using backend.Services;
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
        Response.Cookies.Append("access_token", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Expires = DateTime.UtcNow.AddMinutes(15),
            SameSite = SameSiteMode.Lax
        });
        
        Response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            Expires = DateTime.UtcNow.AddHours(12),
            SameSite = SameSiteMode.Lax
        });
    }

    private async Task RemoveTokenCookies()
    {
        var refreshTokenValue = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refreshTokenValue))
        {
            await _authService.RevokeToken(refreshTokenValue);
        }
        Response.Cookies.Delete("access_token");
        Response.Cookies.Delete("refresh_token");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var loginResponse = await _authService.Login(dto, "Admin");

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
}