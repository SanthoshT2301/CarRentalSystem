using CarRentalSystem.DTO.Maintenance;
using CarRentalSystem.Service.Maintenances;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Maintenance;
[ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly IMaintenanceService _maintenanceService;

        public MaintenanceController(IMaintenanceService maintenanceService)
        {
            _maintenanceService = maintenanceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAlerts()
        {
            var alerts = await _maintenanceService.GetMaintenanceAlertsAsync();
        if (alerts == null)
        {
            return NotFound(new { message = "No alerts found" });
        }
            return Ok(alerts);
        }

        [HttpPost]
        
        public async Task<IActionResult> AddAlert([FromBody] CreateMaintenanceAlertRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var newAlert = await _maintenanceService.AddMaintenanceAlertAsync(request);
                return CreatedAtAction(nameof(GetAlerts), new { id = newAlert.MaintenanceAlertId }, newAlert);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateAlertStatus(int id, [FromBody] UpdateAlertStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new { message = "Status cannot be empty" });
            }

            try
            {
                var updated = await _maintenanceService.UpdateMaintenanceAlertStatusAsync(id, request.Status);
                return Ok(updated);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }

    