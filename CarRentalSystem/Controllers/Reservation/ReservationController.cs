using CarRentalSystem.DTO.Reservation;
using CarRentalSystem.Service.Reservation;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Reservation;
[Route("api/[controller]/[action]")]
[ApiController]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    public ReservationController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyBookings(int userId)
    {
        var reservation=await _reservationService.GetMyBookings(userId);
        if (reservation.Result is NotFoundObjectResult)
        {
            return reservation.Result;
        }
        return Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> CreateBooking(int userId,CreateReservationRequest request)
    {
        var result = await _reservationService.CreateBooking(userId,request);

            if (result.Result is NotFoundObjectResult)
                return result.Result;

            if (result.Result is BadRequestObjectResult)
                return result.Result;

            return Ok(result.Value);
    }
    [HttpDelete("{id}/cancel")]
    public async Task<ActionResult<ReservationDto>> CancelBooking(int id,int userId,bool isAdmin)
    {
        var cancel=await _reservationService.CancelBooking(id,userId,isAdmin);
        if (cancel.Result is NotFoundObjectResult)
        {
            return cancel.Result;
        }
        return Ok(cancel.Value);
    }
     [HttpPut("{id}/return")]
        public async Task<ActionResult<bool>> ReturnCar(int id,int userId,bool isAdmin)
        {
          
            
                var result = await _reservationService.ReturnCarAsync(id, userId, isAdmin);
                if (result.Value == false)
                {
                    return NotFound(new { error = "Booking reservation not found." });
                }
                return Ok(new { message = "Vehicle returned successfully. You can now write a review about the vehicle and our services." });
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllBookingsAsync()
        {
            var reservation=await _reservationService.GetAllBookingsAsync();
            if (reservation.Value.Any())
            {
                return Ok(reservation.Value);
            }
            else
            {
                return NotFound(new { error = "No reservations found." });
            }
        }
}
