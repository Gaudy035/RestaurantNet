using System.ComponentModel.DataAnnotations;
using backend.Data.Entities;

namespace backend.DTOs.Locations;

public class LocationEmployeesResponseDto
{
    public int UserId { get; init; }

    public string FirstName { get; init; } = string.Empty;
    
    public string LastName { get; init; } = string.Empty;

    public int LocationId { get; init; }

    [EnumDataType(typeof(Position))]
    public Position Position { get; init; }
}