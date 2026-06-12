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

 
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var stats = await _adminService.GetAdminStatsAsync();
        return stats.Value is null ? NotFound(new { error = "Stats not found." }) : Ok(stats.Value);
    }

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
        var fileName = $"booking-report-{DateTime.UtcNow:yyyy-MM-dd}.csv";
        return File(csv, "text/csv", fileName);
    }

    [HttpGet("reports/revenue/download")]
    public async Task<IActionResult> DownloadRevenueReport([FromQuery] ReportFilterRequest filter)
    {
        var csv = await _reportService.ExportRevenueReportCsvAsync(filter);
        var fileName = $"revenue-report-{DateTime.UtcNow:yyyy-MM-dd}.csv";
        return File(csv, "text/csv", fileName);
    }

    [HttpGet("reports/reviews/download")]
    public async Task<IActionResult> DownloadReviewReport([FromQuery] ReportFilterRequest filter)
    {
        var csv = await _reportService.ExportReviewReportCsvAsync(filter);
        var fileName = $"review-report-{DateTime.UtcNow:yyyy-MM-dd}.csv";
        return File(csv, "text/csv", fileName);
    }

    [HttpGet("reports/performance/download")]
    public async Task<IActionResult> DownloadPerformanceReport([FromQuery] ReportFilterRequest filter)
    {
        var csv = await _reportService.ExportPerformanceReportCsvAsync(filter);
        var fileName = $"performance-report-{DateTime.UtcNow:yyyy-MM-dd}.csv";
        return File(csv, "text/csv", fileName);
    }
}