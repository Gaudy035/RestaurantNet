using backend.DTOs.Auth;
using backend.DTOs.Users;
using backend.Services.Errors;

namespace backend.Services;

public interface IAuthService
{
    Task RevokeToken(string tokenValue);

    Task<Result<LoginResponseDto>> Login(LoginDto dto, string destination);

    Task<Result<LoginResponseDto>> Refresh(string refreshTokenValue);

    Task<Result<ClientResponseDto>> MeClient(int userId);

    Task<Result<EmployeeResponseDto>> MeAdmin(int userId);
}