namespace backend.DTOs.Users;

public class EmployeeResponseDto
{
    public int UserId { get; set; }
    
    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsAdmin { get; set; }
}