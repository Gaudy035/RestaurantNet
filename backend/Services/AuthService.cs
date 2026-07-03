using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.Data;
using backend.Data.Entities;
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

    private string? GenerateAccessToken(int userId, string userRole)
    {
        string[] roles = ["Admin", "Employee", "Client"];

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

    private async Task<RefreshToken> GenerateRefreshToken(int userId)
    {
        var randomBytes = new byte[64];
        RandomNumberGenerator.Fill(randomBytes);
        var tokenValue = Convert.ToBase64String(randomBytes);

        var newToken = new RefreshToken
        {
            TokenValue = tokenValue,
            IsActive = true,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            UserId = userId
        };

        _context.RefreshTokens.Add(newToken);
        await _context.SaveChangesAsync();

        return newToken;
    }
}