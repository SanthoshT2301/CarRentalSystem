using CarRentalSystem.DTO.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Admin;

public interface IAdminService
{
    Task<ActionResult<AdminStats>> GetAdminStatsAsync();

    /// <summary>Returns all Admin/Agent accounts waiting for approval.</summary>
    Task<List<PendingUserDto>> GetPendingUsersAsync();

    /// <summary>
    /// Approves or rejects a pending Admin/Agent account.
    /// Approve=true  → IsApproved=true  (user can now log in)
    /// Approve=false → IsActive=false   (account is rejected/disabled)
    /// </summary>
    Task<(bool success, string message)> ReviewUserApprovalAsync(int userId, bool approve);
}