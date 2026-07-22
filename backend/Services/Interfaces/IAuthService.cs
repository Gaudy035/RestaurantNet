using backend.DTOs.Auth;
using backend.DTOs.Users;

namespace backend.Services;

public interface IAuthService
{
    Task RevokeToken(string tokenValue);

    Task<LoginResponseDto?> Login(LoginDto dto, string destination);

    Task<LoginResponseDto?> Refresh(string refreshTokenValue);

    Task<CreateClientResponseDto?> MeClient(int userId);

    Task<CreateEmployeeResponseDto?> MeAdmin(int userId);
}