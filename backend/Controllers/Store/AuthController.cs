using System.Security.Claims;
using backend.DTOs.Auth;
using backend.DTOs.Users;
using backend.Services;
using backend.Services.Errors;
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
        var result = await _authService.Login(dto, "Client");

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
        var refreshToken = Request.Cookies["client_refresh_token"];

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

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] ClientCreateDto dto)
    {
        var createClientResponse = await _userService.CreateClient(dto);

        if (!createClientResponse.IsSuccess)
        {
            return createClientResponse.ToActionResult(this);
        }

        var loginDto = new LoginDto
        {
            Email = dto.Email,
            Password = dto.Password
        };

        var loginResponse = await _authService.Login(loginDto, "Client");

        if (!loginResponse.IsSuccess)
        {
            return loginResponse.ToActionResult(this);
        }

        var tokens = loginResponse.Data!;

        CreateTokenCookies(tokens.AccessToken, tokens.RefreshToken);

        return NoContent();
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

        var result = await _authService.MeClient(userId);

        return result.ToActionResult(this);
    }
}