using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Admin;
using CarRentalSystem.DTO.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CarRentalSystem.Models;
namespace CarRentalSystem.Service.Admin;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;

    public AdminService(AppDbContext context) => _context = context;

    // ── Stats ─────────────────────────────────────────────────────────────────
    public async Task<ActionResult<AdminStats>> GetAdminStatsAsync()
    {
        var usersCount = await _context.Users.CountAsync();
        var carsCount = await _context.Cars.CountAsync();
        var bookingsCount = await _context.Reservations.CountAsync();
        var totalRevenue = await _context.Payments
            .Where(p => p.PaymentStatusId == 1)
            .SumAsync(p => p.Amount ?? 0m);

        return new AdminStats
        {
            UsersCount = usersCount,
            CarsCount = carsCount,
            BookingsCount = bookingsCount,
            Revenue = totalRevenue
        };
    }

    // ── Pending approvals ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns Admin and Agent accounts that have not yet been approved.
    /// RoleId 1 = Admin, RoleId 3 = Agent (Customer = 2 — auto-approved, excluded).
    /// </summary>
    public async Task<List<PendingUserDto>> GetPendingUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .Where(u => !u.IsApproved && u.IsActive == true && (u.RoleId == 1 || u.RoleId == 3))
            .OrderBy(u => u.CreatedAt)
            .Select(u => new PendingUserDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role != null ? u.Role.RoleName : "Unknown",
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<(bool success, string message)> ReviewUserApprovalAsync(int userId, bool approve)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        if (user == null)
            return (false, "User not found.");

        // Only Admin/Agent accounts go through the approval flow
        if (user.RoleId == 2) // Customer
            return (false, "Customer accounts do not require approval.");

        if (user.IsApproved)
            return (false, "This account is already approved.");

        if (approve)
        {
            user.IsApproved = true;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, $"{user.Role?.RoleName ?? "User"} account for {user.Email} has been approved.");
        }
        else
        {
            // Rejection: deactivate the account so it cannot be re-submitted
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return (true, $"Account for {user.Email} has been rejected and deactivated.");
        }
    }
    public async Task<bool> SetUserActiveStatusAsync(int userId, bool isActive)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
    // ── All users (for User Management tab) ──────────────────────────────────
    public async Task<List<UserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Include(u => u.Role)
            .OrderBy(u => u.UserId)
            .Select(u => new UserDto
            {
                UserId = u.UserId,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                Phone = u.Phone,
                Role = u.Role != null ? u.Role.RoleName : "Unknown",
                IsActive = u.IsActive ?? true
            })
            .ToListAsync();
    }
    public async Task<(bool success, string message, UserDto? user)> CreateUserAsync(AdminCreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return (false, "Email and password are required.", null);

        var emailLower = request.Email.ToLowerInvariant();
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == emailLower))
            return (false, "A user with this email already exists.", null);

        if (!await _context.Roles.AnyAsync(r => r.RoleId == request.RoleId))
            return (false, "Invalid role specified.", null);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = request.RoleId,
            IsActive = true,
            IsApproved = true, // admin-created accounts skip the approval queue
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var role = await _context.Roles.FindAsync(user.RoleId);
        return (true, "User created successfully.", new UserDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = role?.RoleName ?? "Customer",
            IsActive = user.IsActive ?? true
        });
    }

    public async Task<(bool success, string message, UserDto? user)> UpdateUserAsync(int userId, AdminUpdateUserRequest request)
    {
        var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.UserId == userId);
        if (user == null) return (false, "User not found.", null);

        if (!string.IsNullOrWhiteSpace(request.FirstName)) user.FirstName = request.FirstName.Trim();
        if (request.LastName != null) user.LastName = request.LastName.Trim();

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailLower = request.Email.ToLowerInvariant();
            if (await _context.Users.AnyAsync(u => u.UserId != userId && u.Email.ToLower() == emailLower))
                return (false, "Another user already uses this email.", null);
            user.Email = request.Email.Trim();
        }

        if (request.Phone != null) user.Phone = request.Phone.Trim();

        if (request.RoleId.HasValue)
        {
            if (!await _context.Roles.AnyAsync(r => r.RoleId == request.RoleId.Value))
                return (false, "Invalid role specified.", null);
            user.RoleId = request.RoleId.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var role = await _context.Roles.FindAsync(user.RoleId);
        return (true, "User updated successfully.", new UserDto
        {
            UserId = user.UserId,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Role = role?.RoleName ?? "Customer",
            IsActive = user.IsActive ?? true
        });
    }
}