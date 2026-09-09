using System.ComponentModel.DataAnnotations;
using backend.Data.Entities;

namespace backend.DTOs.Locations;

public class LocationAssignEmployeeDto
{
    [Required]
    public int UserId { get; set; }

    [Required]
    public int LocationId { get; set; }

    [Required]
    [EnumDataType(typeof(Position))]
    public Position Position { get; set; }
}