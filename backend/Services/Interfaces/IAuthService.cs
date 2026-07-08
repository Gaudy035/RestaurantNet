using backend.DTOs.Auth;

namespace backend.Services;

public interface IAuthService
{
    Task RevokeToken(string tokenValue);

    Task<LoginResponseDto?> Login(LoginDto dto, string destination);

    Task<LoginResponseDto?> Refresh(string refreshTokenValue);
}