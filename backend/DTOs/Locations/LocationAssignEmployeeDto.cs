using System.ComponentModel.DataAnnotations;
using backend.Data.Entities;

namespace backend.DTOs.Locations;

public class LocationAssignEmployeeDto
{
    [Required]
    public int UserId { get; init; }

    [Required]
    public int LocationId { get; init; }

    [Required]
    [EnumDataType(typeof(Position))]
    public Position Position { get; init; }
}