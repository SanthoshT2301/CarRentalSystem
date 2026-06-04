using CarRentalSystem.DTO.Common;
using CarRentalSystem.DTO.Reservation;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Reservation;

public interface IReservationService
{
    Task<ActionResult<ReservationDto>> CreateBooking(int userId, CreateReservationRequest request);
    Task<PagedResult<ReservationDto>> GetMyBookings(int userId, int page, int pageSize);
    Task<ActionResult<bool>> CancelBooking(int id, int userId, bool isAdmin);
    Task<ActionResult<bool>> ReturnCarAsync(int id, int userId, bool isAdmin);
    Task<PagedResult<ReservationDto>> GetAllBookingsAsync(int page, int pageSize);
}