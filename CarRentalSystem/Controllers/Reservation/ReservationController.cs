using CarRentalSystem.DTO.Reservation;
using CarRentalSystem.Service.Reservation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Reservation;

[Route("api/[controller]")]
[ApiController]
[Authorize] // All reservation endpoints require a logged-in user at minimum
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationController(IReservationService reservationService)
        => _reservationService = reservationService;

    // Customer / Admin — view own bookings
    [HttpGet("my")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyBookings([FromQuery] int userId)
    {
        var reservations = await _reservationService.GetMyBookings(userId);
        var list = reservations.Value?.ToList();
        return list is { Count: > 0 } ? Ok(list) : NotFound(new { error = "No reservations found." });
    }

    // Customer / Admin — create a booking
    [HttpPost]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<ActionResult<ReservationDto>> CreateBooking([FromQuery] int userId,
                                                                   [FromBody] CreateReservationRequest request)
    {
        var result = await _reservationService.CreateBooking(userId, request);
        if (result.Result is NotFoundObjectResult notFound) return notFound;
        if (result.Result is BadRequestObjectResult bad) return bad;
        return CreatedAtAction(nameof(GetMyBookings), new { userId }, result.Value);
    }

    // Customer / Admin — cancel own booking; Admin can cancel any
    [HttpDelete("{id}/cancel")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> CancelBooking(int id, [FromQuery] int userId, [FromQuery] bool isAdmin = false)
    {
        var result = await _reservationService.CancelBooking(id, userId, isAdmin);
        if (result.Result is NotFoundObjectResult notFound) return notFound;
        return Ok(new { message = "Reservation cancelled successfully." });
    }

    // Agent / Admin — mark vehicle as returned
    [HttpPut("{id}/return")]
    [Authorize(Roles = "Agent,Admin")]
    public async Task<IActionResult> ReturnCar(int id, [FromQuery] int userId, [FromQuery] bool isAdmin = false)
    {
        var result = await _reservationService.ReturnCarAsync(id, userId, isAdmin);
        if (result.Result is NotFoundObjectResult notFound) return notFound;
        return Ok(new { message = "Vehicle returned successfully. The customer may now leave a review." });
    }

    // Admin only — view all bookings across all users
    [HttpGet("all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllBookings()
    {
        var reservations = await _reservationService.GetAllBookingsAsync();
        var list = reservations.Value?.ToList();
        return list is { Count: > 0 } ? Ok(list) : NotFound(new { error = "No reservations found." });
    }
}