using CarRentalSystem.DTO.Reservation;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Reservation;
public interface IReservationService
{
    Task<ActionResult<ReservationDto>> CreateBooking(int userId,CreateReservationRequest request);
    Task<ActionResult<IEnumerable<ReservationDto>>> GetMyBookings(int userId);
    Task<ActionResult<bool>> CancelBooking(int id,int userId,bool isAdmin);
    Task<ActionResult<bool>> ReturnCarAsync(int id, int userId, bool isAdmin);
    Task<ActionResult<IEnumerable<ReservationDto>>> GetAllBookingsAsync();
}