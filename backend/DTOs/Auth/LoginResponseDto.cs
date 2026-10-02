namespace backend.DTOs.Auth;

public class LoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}