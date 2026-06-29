using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Payment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Controllers.v1;

[ApiController]
[Route("api/v1/payments")]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly AppDbContext _context;
    public PaymentController(AppDbContext context) => _context = context;

    [HttpGet("my")]
    [Authorize(Roles = "Customer,Admin")]
    public async Task<IActionResult> GetMyPaymentHistory([FromQuery] int userId)
    {
        var payments = await _context.Payments
            .Include(p => p.Reservation).ThenInclude(r => r!.Car).ThenInclude(c => c!.Brand)
            .Include(p => p.PaymentMethod)
            .Include(p => p.PaymentStatus)
            .Where(p => p.Reservation != null && p.Reservation.UserId == userId)
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentHistoryDto
            {
                PaymentId = p.PaymentId,
                ReservationId = p.ReservationId,
                CarName = p.Reservation!.Car != null
                    ? $"{(p.Reservation.Car.Brand != null ? p.Reservation.Car.Brand.BrandName : "")} {p.Reservation.Car.Model}"
                    : "N/A",
                Amount = p.Amount ?? 0,
                PaymentMethod = p.PaymentMethod != null ? p.PaymentMethod.MethodName : "N/A",
                PaymentStatus = p.PaymentStatus != null ? p.PaymentStatus.StatusName : "N/A",
                TransactionId = p.TransactionId,
                PaymentDate = p.PaymentDate.HasValue ? p.PaymentDate.Value.ToString("yyyy-MM-dd HH:mm") : "N/A"
            })
            .ToListAsync();

        return Ok(payments);
    }
}