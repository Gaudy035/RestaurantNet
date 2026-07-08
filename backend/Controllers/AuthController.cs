using backend.DTOs.Auth;
using backend.DTOs.Users;
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

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh()
    {
        var refreshToken = Request.Cookies["refresh_token"];

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
    public async Task<IActionResult> Register([FromBody] CreateClientDto dto)
    {
        var createClientResponse = await _userService.CreateClient(dto);

        if (createClientResponse == null)
        {
            return BadRequest();
        }

        var loginDto = new LoginDtod
        {
            Email = dto.Email,
            Password = dto.Password
        };

        var loginResponse = await _authService.Login(loginDto, "Client");

        CreateTokenCookies(loginResponse!.AccessToken, loginResponse!.RefreshToken);

        return Ok(new { message = "Registered successfully" });
    }
}