using CarRentalSystem.DTO.Check;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Logistics;
    [ApiController]
    [Route("api/[controller]")]

    public class GateLogisticsController : ControllerBase
    {
        private readonly IGateLogisticsService _gateLogisticsService;

        public GateLogisticsController(IGateLogisticsService gateLogisticsService)
        {
            _gateLogisticsService = gateLogisticsService;
        }

        [HttpPost("checkout/{reservationId}")]
        public async Task<IActionResult> ChecklistCheckout(int reservationId, [FromBody] GateCheckoutRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var checkout = await _gateLogisticsService.VerifyAndCheckoutAsync(reservationId, request);
                return Ok(checkout);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("checkin/{reservationId}")]
        public async Task<IActionResult> ChecklistCheckin(int reservationId, [FromBody] GateCheckinRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var checkin = await _gateLogisticsService.VerifyAndCheckinAsync(reservationId, request);
                return Ok(checkin);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("details/{reservationId}")]

        public async Task<IActionResult> GetLogs(int reservationId)
        {
            var checkoutObj = await _gateLogisticsService.GetCheckoutDetailsByReservationAsync(reservationId);
            var checkinObj = await _gateLogisticsService.GetCheckinDetailsByReservationAsync(reservationId);

            return Ok(new GateLogisticsLogsDto
            {
                Checkout = checkoutObj,
                Checkin = checkinObj
            });
        }
    }