using backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

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
}