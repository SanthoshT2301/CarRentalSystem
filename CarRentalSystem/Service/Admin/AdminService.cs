using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Admin;
using CarRentalSystem.DTO.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
}