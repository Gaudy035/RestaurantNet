using backend.Data.Entities;
using backend.DTOs.Users;

namespace backend.Services;

public interface IUserService
{
    Task<CreateClientResponseDto?> CreateClient(CreateClientDto dto);

    Task<CreateEmployeeResponseDto?> CreateEmployee(CreateEmployeeDto dto);
}