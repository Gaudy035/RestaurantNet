using backend.Data;
using backend.Data.Entities;
using backend.DTOs.Users;
using backend.Services.Errors;
using Microsoft.EntityFrameworkCore;

namespace backend.Services;

public class UserService: IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<ClientResponseDto>> CreateClient(ClientCreateDto dto)
    {
        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);
        
        if (emailTaken)
        {
            return Result<ClientResponseDto>.Fail(Error.From(ErrorCode.EmailAlreadyTaken));
        }

        var hashedPass = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var newClient = _context.Clients.Add(new Client
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
            return Result<ClientResponseDto>.Fail(Error.From(ErrorCode.DbOperationFailed));
        }

        return Result<ClientResponseDto>.Success(new ClientResponseDto
        {
            UserId = newClient.Entity.UserId,
            FirstName = newClient.Entity.User.FirstName,
            LastName = newClient.Entity.User.LastName,
            Email = newClient.Entity.User.Email,
            PhoneNumber = newClient.Entity.PhoneNumber
        });
    }
   
    public async Task<Result<IEnumerable<ClientResponseDto>>> FindClient(string? parameter)
    {
        var query = _context.Clients.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameter))
        {
            var par = $"%{parameter.ToLower()}%";
            query = query.Where(c => 
                EF.Functions.Like(c.User.FirstName.ToLower() + " " + c.User.LastName.ToLower(), par) ||
                EF.Functions.Like(c.User.LastName.ToLower() + " " + c.User.FirstName.ToLower(), par) ||
                EF.Functions.Like(c.User.Email.ToLower(), par)
            );
        }

        var clients = await query.OrderBy(c => c.User.LastName)
            .ThenBy(c => c.User.FirstName)
            .Select(c => new ClientResponseDto
            {
                UserId = c.UserId,
                FirstName = c.User.FirstName,
                LastName = c.User.LastName,
                Email = c.User.Email,
                PhoneNumber = c.PhoneNumber
            }).ToListAsync();

        return Result<IEnumerable<ClientResponseDto>>.Success(clients);
    }

    public async Task<Result<ClientResponseDto>> FindClientById(int clientId)
    {
        var client = await _context.Clients
            .AsNoTracking()
            .Where(c => c.UserId == clientId)
            .Select(c => new ClientResponseDto
            {
                UserId = c.UserId,
                FirstName = c.User.FirstName,
                LastName = c.User.LastName,
                Email = c.User.Email,
                PhoneNumber = c.PhoneNumber
            }).SingleOrDefaultAsync();

        return client == null 
            ? Result<ClientResponseDto>.Fail(Error.From(ErrorCode.ClientNotFound))
            : Result<ClientResponseDto>.Success(client);
    }

    public async Task<bool> DeleteClient(int clientId)
    {
        var client = await _context.Clients
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.UserId == clientId);

        if (client == null)
        {
            return false;
        }

        _context.Users.Remove(client.User);

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task<EmployeeResponseDto?> CreateEmployee (EmployeeCreateDto dto)
    {
        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == dto.Email);
        
        if (emailTaken)
        {
            return null;
        }

        var hashedPass = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        var newEmployee = _context.Employees.Add(new Employee
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

    public async Task<IEnumerable<EmployeeResponseDto>> FindEmployee(string? parameter)
    {
        var query = _context.Employees.AsNoTracking().AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(parameter))
        {
            var par = $"%{parameter.ToLower()}%";
            query = query.Where(e =>
                EF.Functions.Like(e.User.FirstName.ToLower() + " " + e.User.LastName.ToLower(), par) ||
                EF.Functions.Like(e.User.LastName.ToLower() + " " + e.User.FirstName.ToLower(), par) ||
                EF.Functions.Like(e.User.Email.ToLower(), par)
            );
        }

        return await query.OrderBy(e => e.User.LastName)
            .ThenBy(e => e.User.FirstName)
            .Select(e => new EmployeeResponseDto
            {
                UserId = e.UserId,
                FirstName = e.User.FirstName,
                LastName = e.User.LastName,
                Email = e.User.Email,
                IsAdmin = e.IsAdmin
            }).ToListAsync();
    }

    public async Task<EmployeeResponseDto?> FindEmployeeById(int employeeId)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(e => e.UserId == employeeId)
            .Select(e => new EmployeeResponseDto
            {
                UserId = e.UserId,
                FirstName = e.User.FirstName,
                LastName = e.User.LastName,
                Email = e.User.Email,
                IsAdmin = e.IsAdmin
            }).SingleOrDefaultAsync();
    }

    public async Task<IEnumerable<EmployeeLocationsResponseDto>> GetEmployeeLocations(int employeeId)
    {
        return await _context.LocationEmployees
            .AsNoTracking()
            .Where(le => le.UserId == employeeId)
            .OrderBy(le => le.Location.City)
            .ThenBy(le => le.Location.Address)
            .ThenBy(le => le.Position)
            .Select(le => new EmployeeLocationsResponseDto
            {
                LocationId = le.LocationId,
                City = le.Location.City,
                Address = le.Location.Address,
                UserId = le.UserId,
                Position = le.Position
            }).ToListAsync();
    }
    public async Task<bool> DeleteEmployee(int employeeId)
    {
        var employee = await _context.Employees
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.UserId == employeeId);

        if (employee == null)
        {
            return false;
        }

        _context.Users.Remove(employee.User);

        try
        {
            await _context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
}