using CarRentalSystem.Service.Admin;
using CarRentalSystem.Service.Reservation;
using CarRentalSystem.Service.Review;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Admin;
    [ApiController]
    [Route("api/admin")]
    
    public class AdminController : ControllerBase
    {
        private readonly IReservationService _bookingService;
        private readonly IAdminService _adminService;
        private readonly IReviewService _reviewService;

        public AdminController(
            IReservationService bookingService,
            IAdminService adminService,
            IReviewService reviewService)
        {
            _bookingService = bookingService;
            _adminService = adminService;
            _reviewService = reviewService;
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