using CarRentalSystem.Service.Admin;
using CarRentalSystem.Service.Reservation;
using CarRentalSystem.Service.Review;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Admin;
    [ApiController]
    [Route("api/admin")]
    
    public class AdminController : ControllerBase
    {

        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            
            _adminService = adminService;
            
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _adminService.GetAdminStatsAsync();
        if (stats == null)
        {
            return NotFound(new { error = "Admin stats not found." });
        }
            return Ok(stats);
        }
}