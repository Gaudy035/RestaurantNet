using System.ComponentModel.DataAnnotations;
using backend.Data.Entities;

namespace backend.DTOs.Locations;

public class LocationEmployeesResponseDto
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    
    public string LastName { get; set; } = string.Empty;

    public int LocationId { get; set; }

    [EnumDataType(typeof(Position))]
    public Position Position { get; set; }
}