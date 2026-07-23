using System.Text.RegularExpressions;
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

    public async Task<IEnumerable<ClientResponseDto>> FindClient(string? parameter)
    {
        var query = _context.Clients.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameter))
        {
            var par = $"%{parameter}%";
            query = query.Where(c => 
                EF.Functions.ILike($"{c.User.FirstName} {c.User.LastName}", par) ||
                EF.Functions.ILike($"{c.User.LastName} {c.User.FirstName}", par) ||
                EF.Functions.ILike(c.User.Email, par)
            );
        }

        return await query.Select(c => new ClientResponseDto
        {
            UserId = c.UserId,
            FirstName = c.User.FirstName,
            LastName = c.User.LastName,
            Email = c.User.Email,
            PhoneNumber = c.PhoneNumber
        }).ToListAsync();
    }

    public async Task<IEnumerable<EmployeeResponseDto>> FindEmployee(string? parameter)
    {
        var query = _context.Employees.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(parameter))
        {
            var par = $"%{parameter}%";
            query = query.Where(e =>
                EF.Functions.ILike($"{e.User.FirstName} {e.User.LastName}", par) ||
                EF.Functions.ILike($"{e.User.LastName} {e.User.FirstName}", par) ||
                EF.Functions.ILike(e.User.Email, par)
            );
        }

        return await query.Select(e => new EmployeeResponseDto
        {
            UserId = e.UserId,
            FirstName = e.User.FirstName,
            LastName = e.User.LastName,
            Email = e.User.Email,
            IsAdmin = e.IsAdmin
        }).ToListAsync();
    }
}