using CarRentalSystem.DTO.Admin;
using CarRentalSystem.DTO.User;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Admin;

public interface IAdminService
{
    Task<ActionResult<AdminStats>> GetAdminStatsAsync();

    /// <summary>Returns all Admin/Agent accounts waiting for approval.</summary>
    Task<List<PendingUserDto>> GetPendingUsersAsync();
    Task<List<UserDto>> GetAllUsersAsync();
    Task<(bool success, string message)> ReviewUserApprovalAsync(int userId, bool approve);
    Task<bool> SetUserActiveStatusAsync(int userId, bool isActive);
    Task<bool> DeleteUserAsync(int userId);
    Task<(bool success, string message, UserDto? user)> CreateUserAsync(AdminCreateUserRequest request);
    Task<(bool success, string message, UserDto? user)> UpdateUserAsync(int userId, AdminUpdateUserRequest request);
}