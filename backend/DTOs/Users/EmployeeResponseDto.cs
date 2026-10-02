namespace backend.DTOs.Users;

public class EmployeeResponseDto
{
    public int UserId { get; init; }
    
    public string FirstName { get; init; } = string.Empty;
    
    public string LastName { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsAdmin { get; init; }
}