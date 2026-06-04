using CarRentalSystem.DTO.Maintenance;
using CarRentalSystem.Service.Maintenances;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers.v1;

[ApiController]
[Route("api/v1/maintenance")]
[Authorize(Roles = "Admin,Agent")]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;
    public MaintenanceController(IMaintenanceService maintenanceService) => _maintenanceService = maintenanceService;

    [HttpGet]
    public async Task<IActionResult> GetAlerts()
    {
        var alerts = await _maintenanceService.GetMaintenanceAlertsAsync();
        return alerts is null ? NotFound(new { message = "No alerts found." }) : Ok(alerts);
    }

    [HttpPost]
    public async Task<IActionResult> AddAlert([FromBody] CreateMaintenanceAlertRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var newAlert = await _maintenanceService.AddMaintenanceAlertAsync(request);
        return CreatedAtAction(nameof(GetAlerts), new { id = newAlert.MaintenanceAlertId }, newAlert);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateAlertStatus(int id, [FromBody] UpdateAlertStatusRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Status)) return BadRequest(new { message = "Status cannot be empty." });
        var updated = await _maintenanceService.UpdateMaintenanceAlertStatusAsync(id, request.Status);
        return Ok(updated);
    }
}