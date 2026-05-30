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
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyBookings()
    {
        var reservation=await _reservationService.GetMyBookings();
        if (reservation.Result is NotFoundObjectResult)
        {
            return reservation.Result;
        }
        return Ok(reservation);
    }

    [HttpPost]
    public async Task<ActionResult<ReservationDto>> CreateBooking(CreateReservationRequest request)
    {
        var result = await _reservationService.CreateBooking(request);

            if (result.Result is NotFoundObjectResult)
                return result.Result;

            if (result.Result is BadRequestObjectResult)
                return result.Result;

            return Ok(result.Value);
    }
    


}
