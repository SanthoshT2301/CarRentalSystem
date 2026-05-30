using CarRentalSystem.DTO.Reservation;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Reservation;
public interface IReservationService
{
    Task<ActionResult<ReservationDto>> CreateBooking(CreateReservationRequest request);
    Task<ActionResult<IEnumerable<ReservationDto>>> GetMyBookings();
    Task<ActionResult<ReservationDto>> CancelBooking(int id);
}