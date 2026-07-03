using backend.Data.Entities;
using backend.DTOs.Users;

namespace backend.Services;

public interface IUserService
{
    Task<Client?> CreateClient(CreateClientDto dto);

    Task<Employee?> CreateEmployee(CreateEmployeeDto dto);
}