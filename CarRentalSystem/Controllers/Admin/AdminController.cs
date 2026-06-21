using CarRentalSystem.DTO.Admin;
using CarRentalSystem.DTO.Reports;
using CarRentalSystem.Service.Admin;
using CarRentalSystem.Service.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers.v1;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly IReportService _reportService;

    public AdminController(IAdminService adminService, IReportService reportService)
    {
        _adminService = adminService;
        _reportService = reportService;
    }

    // ── Dashboard stats ───────────────────────────────────────────────────────
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetAdminStatsAsync();
        return stats.Value is null ? NotFound(new { error = "Stats not found." }) : Ok(stats.Value);
    }

    // ── User approval ─────────────────────────────────────────────────────────

    /// <summary>
    /// GET api/v1/admin/users/pending
    /// Returns all Admin/Agent accounts awaiting approval.
    /// </summary>
    [HttpGet("users/pending")]
    public async Task<IActionResult> GetPendingUsers()
    {
        var pending = await _adminService.GetPendingUsersAsync();
        return Ok(new { count = pending.Count, data = pending });
    }

    /// <summary>
    /// PUT api/v1/admin/users/{userId}/approve
    /// Body: { "approve": true }  → approves the account
    /// Body: { "approve": false } → rejects / deactivates the account
    /// </summary>
    [HttpPut("users/{userId}/approve")]
    public async Task<IActionResult> ReviewUserApproval(int userId, [FromBody] ApproveUserRequest request)
    {
        var (success, message) = await _adminService.ReviewUserApprovalAsync(userId, request.Approve);

        if (!success)
            return BadRequest(new { error = message });

        return Ok(new { message });
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(new { count = users.Count, data = users });
    }

    // ── Reports ───────────────────────────────────────────────────────────────
    [HttpGet("reports/bookings")]
    public async Task<IActionResult> GetBookingReport([FromQuery] ReportFilterRequest filter)
    {
        var data = await _reportService.GetBookingReportAsync(filter);
        return Ok(new { count = data.Count, data });
    }

    [HttpGet("reports/revenue")]
    public async Task<IActionResult> GetRevenueReport([FromQuery] ReportFilterRequest filter)
    {
        var data = await _reportService.GetRevenueReportAsync(filter);
        return Ok(new { count = data.Count, data });
    }

    [HttpGet("reports/reviews")]
    public async Task<IActionResult> GetReviewReport([FromQuery] ReportFilterRequest filter)
    {
        var data = await _reportService.GetReviewReportAsync(filter);
        return Ok(new { count = data.Count, data });
    }

    [HttpGet("reports/performance")]
    public async Task<IActionResult> GetPerformanceReport([FromQuery] ReportFilterRequest filter)
    {
        var data = await _reportService.GetPerformanceReportAsync(filter);
        return Ok(new { count = data.Count, data });
    }

    [HttpGet("reports/bookings/download")]
    public async Task<IActionResult> DownloadBookingReport([FromQuery] ReportFilterRequest filter)
    {
        var csv = await _reportService.ExportBookingReportCsvAsync(filter);
        return File(csv, "text/csv", $"booking-report-{DateTime.UtcNow:yyyy-MM-dd}.csv");
    }

    [HttpGet("reports/revenue/download")]
    public async Task<IActionResult> DownloadRevenueReport([FromQuery] ReportFilterRequest filter)
    {
        var csv = await _reportService.ExportRevenueReportCsvAsync(filter);
        return File(csv, "text/csv", $"revenue-report-{DateTime.UtcNow:yyyy-MM-dd}.csv");
    }

    [HttpGet("reports/reviews/download")]
    public async Task<IActionResult> DownloadReviewReport([FromQuery] ReportFilterRequest filter)
    {
        var csv = await _reportService.ExportReviewReportCsvAsync(filter);
        return File(csv, "text/csv", $"review-report-{DateTime.UtcNow:yyyy-MM-dd}.csv");
    }

    [HttpGet("reports/performance/download")]
    public async Task<IActionResult> DownloadPerformanceReport([FromQuery] ReportFilterRequest filter)
    {
        var csv = await _reportService.ExportPerformanceReportCsvAsync(filter);
        return File(csv, "text/csv", $"performance-report-{DateTime.UtcNow:yyyy-MM-dd}.csv");
    }

    [HttpPatch("users/{id}/status")]
    public async Task<IActionResult> SetUserActiveStatus(int id, [FromQuery] bool isActive)
    {
        var success = await _adminService.SetUserActiveStatusAsync(id, isActive);
        return success
            ? Ok(new { message = $"User {(isActive ? "activated" : "deactivated")} successfully." })
            : NotFound(new { error = $"User {id} not found." });
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var success = await _adminService.DeleteUserAsync(id);
        return success
            ? Ok(new { message = "User deleted successfully." })
            : NotFound(new { error = $"User {id} not found." });
    }
}