using System.ComponentModel.DataAnnotations;
using backend.Data.Entities;

namespace backend.DTOs.Users;

public class EmployeeLocationsResponseDto
{
    public int LocationId { get; set; }

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public int UserId { get; set; }

    [EnumDataType(typeof(Position))]
    public Position Position { get; set; }
}