using CarRentalSystem.DTO.Check;
using CarRentalSystem.Service.Logistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers.v1;

[ApiController]
[Route("api/v1/gate")]
[Authorize(Roles = "Admin,Agent")]
public class GateLogisticsController : ControllerBase
{
    private readonly IGateLogisticsService _gateLogisticsService;
    public GateLogisticsController(IGateLogisticsService gateLogisticsService) => _gateLogisticsService = gateLogisticsService;

    [HttpPost("checkout/{reservationId}")]
    public async Task<IActionResult> ChecklistCheckout(int reservationId, [FromBody] GateCheckoutRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var checkout = await _gateLogisticsService.VerifyAndCheckoutAsync(reservationId, request);
        return Ok(checkout);
    }

    [HttpPost("checkin/{reservationId}")]
    public async Task<IActionResult> ChecklistCheckin(int reservationId, [FromBody] GateCheckinRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var checkin = await _gateLogisticsService.VerifyAndCheckinAsync(reservationId, request);
        return Ok(checkin);
    }

    [HttpGet("details/{reservationId}")]
    public async Task<IActionResult> GetLogs(int reservationId)
    {
        var checkout = await _gateLogisticsService.GetCheckoutDetailsByReservationAsync(reservationId);
        var checkin = await _gateLogisticsService.GetCheckinDetailsByReservationAsync(reservationId);
        return Ok(new GateLogisticsLogsDto { Checkout = checkout, Checkin = checkin });
    }
}