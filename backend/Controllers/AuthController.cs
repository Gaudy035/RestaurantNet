using backend.DTOs.Auth;
using backend.Services;
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
            Expires = DateTime.UtcNow.AddDays(7),
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
}