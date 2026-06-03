using CarRentalSystem.Service.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")] // Every endpoint in this controller is Admin-only
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService) => _adminService = adminService;

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetAdminStatsAsync();
        return stats.Value is null
            ? NotFound(new { error = "Admin stats not found." })
            : Ok(stats.Value);
    }
}