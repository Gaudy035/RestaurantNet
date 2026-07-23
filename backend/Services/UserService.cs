using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Users;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserService: IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ClientResponseDto?> CreateClient(CreateClientDto dto)
    {
        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);
        
        if (emailTaken)
        {
            return null;
        }

        var hashedPass = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var newClient = await _context.Clients.AddAsync(new Client
        {
            PhoneNumber = dto.PhoneNumber,
            User = new User
            {      
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = hashedPass   
            }
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return new ClientResponseDto
        {
            UserId = newClient.Entity.UserId,
            FirstName = newClient.Entity.User.FirstName,
            LastName = newClient.Entity.User.LastName,
            Email = newClient.Entity.User.Email,
            PhoneNumber = newClient.Entity.PhoneNumber
        };
    }

    public async Task<EmployeeResponseDto?> CreateEmployee (CreateEmployeeDto dto)
    {
        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);
        
        if (emailTaken)
        {
            return null;
        }

        var hashedPass = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var newEmployee = await _context.Employees.AddAsync(new Employee
        {
            IsAdmin = dto.IsAdmin,
            User = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Password = hashedPass 
            }
        });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return null;
        }

        return new EmployeeResponseDto
        {
            UserId = newEmployee.Entity.UserId,
            FirstName = newEmployee.Entity.User.FirstName,
            LastName = newEmployee.Entity.User.LastName,
            Email = newEmployee.Entity.User.Email,
            IsAdmin = newEmployee.Entity.IsAdmin
        };
    }
}