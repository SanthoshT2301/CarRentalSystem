using CarRentalSystem.DTO.Common;
using CarRentalSystem.DTO.Reservation;
using CarRentalSystem.Service.Reservation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers.v1;

[Route("api/v1/reservations")]
[ApiController]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
        => _reservationService = reservationService;

    /// <summary>Customer / Admin — view own bookings (paginated).</summary>
    [HttpGet("my")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetMyBookings(
        [FromQuery] int userId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reservationService.GetMyBookings(userId, page, pageSize);
        return Ok(result);
    }

    /// <summary>Admin only — view all bookings (paginated).</summary>
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResult<ReservationDto>>> GetAllBookings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reservationService.GetAllBookingsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Customer / Admin — create a booking.</summary>
    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<ReservationDto>> CreateBooking(
        [FromQuery] int userId,
        [FromBody] CreateReservationRequest request)
    {
        var result = await _reservationService.CreateBooking(userId, request);
        if (result.Result is NotFoundObjectResult notFound) return notFound;
        if (result.Result is BadRequestObjectResult bad) return bad;
        return CreatedAtAction(nameof(GetMyBookings), new { userId }, result.Value);
    }

    /// <summary>Customer / Admin — cancel a booking.</summary>
    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> CancelBooking(
        int id,
        [FromQuery] int userId,
        [FromQuery] bool isAdmin = false)
    {
        var result = await _reservationService.CancelBooking(id, userId, isAdmin);
        if (result.Result is NotFoundObjectResult notFound) return notFound;
        return Ok(new { message = "Reservation cancelled successfully." });
    }

    /// <summary>Agent / Admin — mark vehicle as returned.</summary>
    [HttpPut("{id}/return")]
    [Authorize(Roles = "Agent,Admin")]
    public async Task<IActionResult> ReturnCar(
        int id,
        [FromQuery] int userId,
        [FromQuery] bool isAdmin = false)
    {
        var result = await _reservationService.ReturnCarAsync(id, userId, isAdmin);
        if (result.Result is NotFoundObjectResult notFound) return notFound;
        return Ok(new { message = "Vehicle returned successfully." });
    }
}