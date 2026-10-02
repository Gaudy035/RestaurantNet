using System.ComponentModel.DataAnnotations;
using backend.Data.Entities;

namespace backend.DTOs.Users;

public class EmployeeLocationsResponseDto
{
    public int LocationId { get; init; }

    public string City { get; init; } = string.Empty;

    public string Address { get; init; } = string.Empty;

    public int UserId { get; init; }

    [EnumDataType(typeof(Position))]
    public Position Position { get; init; }
}