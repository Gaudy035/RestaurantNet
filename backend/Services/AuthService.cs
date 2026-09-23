using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Auth;
using backend.DTOs.Users;
using backend.Services.Errors;
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

    private string GenerateAccessToken(int userId, string userRole)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var keyStr = _configuration["Jwt:Key"];
        var key = Encoding.UTF8.GetBytes(keyStr!);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
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

    private async Task<string> GenerateRefreshToken(int userId, string role)
    {
        var expiration = role == "Client" 
            ? DateTimeOffset.UtcNow.AddDays(7) 
            : DateTimeOffset.UtcNow.AddHours(12);

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
            refreshToken.RevokedAt = DateTimeOffset.UtcNow;            
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Result<LoginResponseDto>> Login(LoginDto dto, string destination)
    {
        var foundUser = await _context.Users
            .AsNoTracking()
            .Include(u => u.Client)
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email == dto.Email);
        
        if (foundUser == null)
        {
            return Result<LoginResponseDto>.Fail(ErrorCode.InvalidCredentials);
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
            return Result<LoginResponseDto>.Fail(ErrorCode.InvalidCredentials);
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, foundUser.Password))
        {
            return Result<LoginResponseDto>.Fail(ErrorCode.InvalidCredentials);
        }

        try
        {
            var accessToken = GenerateAccessToken(foundUser.UserId, role);
            var refreshToken = await GenerateRefreshToken(foundUser.UserId, role);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Role = role
            });
        }
        catch (DbUpdateException)
        {
            return Result<LoginResponseDto>.Fail(ErrorCode.DbOperationFailed);
        }
    }

    public async Task<Result<LoginResponseDto>> Refresh(string refreshTokenValue)
    {
        var oldToken = await _context.RefreshTokens
            .AsNoTracking()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenValue == refreshTokenValue);

        if (oldToken == null || !oldToken.IsActive)
        {
            return Result<LoginResponseDto>.Fail(ErrorCode.InvalidRefreshToken);
        }

        try
        {
            await RevokeToken(oldToken.TokenValue);

            if (oldToken.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                return Result<LoginResponseDto>.Fail(ErrorCode.InvalidRefreshToken);
            }
            var newAccessToken = GenerateAccessToken(oldToken.UserId, oldToken.Role);
            var newRefreshToken = await GenerateRefreshToken(oldToken.UserId, oldToken.Role);

            return Result<LoginResponseDto>.Success(new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Role = oldToken.Role
            });
        }
        catch (DbUpdateException)
        {
            return Result<LoginResponseDto>.Fail(ErrorCode.DbOperationFailed);
        }
    }

    public async Task<Result<ClientResponseDto>> MeClient(int userId)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        
        if (client == null)
        {
            return Result<ClientResponseDto>.Fail(ErrorCode.ClientNotFound);
        }

        return Result<ClientResponseDto>.Success(new ClientResponseDto
        {
            UserId = client.UserId,
            FirstName = client.User.FirstName,
            LastName = client.User.LastName,
            Email = client.User.Email,
            PhoneNumber = client.PhoneNumber
        });
    }

    public async Task<Result<EmployeeResponseDto>> MeAdmin(int userId)
    {
        var employee = await _context.Employees
            .AsNoTracking()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.UserId == userId);

        if (employee == null)
        {
            return Result<EmployeeResponseDto>.Fail(ErrorCode.EmployeeNotFound);
        }

        return Result<EmployeeResponseDto>.Success(new EmployeeResponseDto
        {
            UserId = employee.UserId,
            FirstName = employee.User.FirstName,
            LastName = employee.User.LastName,
            Email = employee.User.Email,
            IsAdmin = employee.IsAdmin
        });
    }
}