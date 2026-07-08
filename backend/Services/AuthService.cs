using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public class AuthService: IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    private static readonly string[] roles = ["Admin", "Employee", "Client"];

    private string? GenerateAccessToken(int userId, string userRole)
    {
        if (!roles.Contains(userRole))
        {
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();

        var keyStr = _configuration["Jwt:Key"];
        var key = Encoding.UTF8.GetBytes(keyStr!);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, userRole)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private async Task<string?> GenerateRefreshToken(int userId, string role)
    {
        if (!roles.Contains(role))
        {
            return null;
        }

        var expiration = role == "Client" 
            ? DateTime.UtcNow.AddDays(7) 
            : DateTime.UtcNow.AddHours(12);

        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        var tokenValue = Convert.ToBase64String(randomBytes);

        var newToken = new RefreshToken
        {
            TokenValue = tokenValue,
            IsActive = true,
            ExpiresAt = expiration,
            UserId = userId,
            Role = role
        };

        _context.RefreshTokens.Add(newToken);
        await _context.SaveChangesAsync();

        return newToken.TokenValue;
    }

    public async Task RevokeToken(string tokenValue)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenValue == tokenValue);
        
        if (refreshToken == null)
        {
            return;
        }

        if (refreshToken.IsActive)
        {
            refreshToken.IsActive = false;
            refreshToken.RevokedAt = DateTime.UtcNow;            
        }

        await _context.SaveChangesAsync();
    }

    public async Task<LoginResponseDto?> Login(LoginDto dto, string destination)
    {
        var foundUser = await _context.Users
            .Include(u => u.Client)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        
        if (foundUser == null)
        {
            return null;
        }
        
        string role;

        if (foundUser.Client != null && destination == "Client")
        {
            role = "Client";
        } 
        else if (foundUser.Employee != null && destination == "Admin")
        {
            role = foundUser.Employee.IsAdmin 
                ? "Admin" 
                : "Employee";
        }
        else
        {
            return null;
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, foundUser.Password))
        {
            return null;
        }

        var accessToken = GenerateAccessToken(foundUser.UserId, role);
        var refreshToken = await GenerateRefreshToken(foundUser.UserId, role);

        if (accessToken == null || refreshToken == null)
        {
            return null;
        }

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Role = role
        };
    }

    public async Task<LoginResponseDto?> Refresh(string refreshTokenValue)
    {
        var oldToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenValue == refreshTokenValue);

        if (oldToken == null || !oldToken.IsActive)
        {
            return null;
        }

        await RevokeToken(oldToken.TokenValue);

        if (oldToken.ExpiresAt <= DateTime.UtcNow)
        {
            return null;
        }

        var newAccessToken = GenerateAccessToken(oldToken.UserId, oldToken.Role);
        var newRefreshToken = await GenerateRefreshToken(oldToken.UserId, oldToken.Role);

        if (newAccessToken == null || newRefreshToken == null)
        {
            return null;
        }

        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Role = oldToken.Role
        };
    }
}