namespace backend.Services;

public interface IAuthService
{
    Task RevokeToken(string tokenValue);
}