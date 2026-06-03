using AutoMapper;
using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Reservation;
using CarRentalSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Reservation;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ReservationService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // ── CREATE BOOKING ────────────────────────────────────────────────────────
    public async Task<ActionResult<ReservationDto>> CreateBooking(int userId, CreateReservationRequest request)
    {
        var car = await _context.Cars.FindAsync(request.CarId);
        if (car == null) return new NotFoundObjectResult("Car not found.");

        var pickupLoc = await _context.Locations
            .FirstOrDefaultAsync(l => l.LocationName == request.PickupLocation);
        if (pickupLoc == null) return new NotFoundObjectResult("Pickup location not found.");

        var dropoffLoc = await _context.Locations
            .FirstOrDefaultAsync(l => l.LocationName == request.DropoffLocation);
        if (dropoffLoc == null) return new NotFoundObjectResult("Dropoff location not found.");

        // ── Date resolution (daily vs hourly) ────────────────────────────────
        DateTime pDate, dDate;

        if (request.IsHourly)
        {
            DateTime.TryParse(request.PickupDate, out var parsedDate);
            if (parsedDate == DateTime.MinValue) parsedDate = DateTime.UtcNow.Date;

            pDate = TimeSpan.TryParse(request.PickupTime, out var pTime)
                ? parsedDate.Date.Add(pTime)
                : parsedDate.Date.AddHours(12);

            dDate = pDate.AddHours(request.DurationHours > 0 ? request.DurationHours : 1);
        }
        else
        {
            DateTime.TryParse(request.PickupDate, out pDate);
            DateTime.TryParse(request.DropoffDate, out dDate);

            if (pDate == DateTime.MinValue) pDate = DateTime.UtcNow;
            if (dDate == DateTime.MinValue) dDate = pDate.AddDays(3);

            if ((dDate - pDate).Days <= 0)
                return new BadRequestObjectResult("Dropoff date must be after pickup date.");
        }

        // ── Overlap check ────────────────────────────────────────────────────
        var isBooked = await _context.Reservations.AnyAsync(r =>
            r.CarId == request.CarId &&
            r.ReservationStatusId == 1 &&
            ((pDate >= r.PickupDate && pDate < r.DropDate) ||
             (dDate > r.PickupDate && dDate <= r.DropDate) ||
             (pDate <= r.PickupDate && dDate >= r.DropDate)));

        if (isBooked)
            throw new InvalidOperationException(
                "This vehicle is already booked for the requested period. Please choose another car or a different time window.");

        // ── Amount calculation ────────────────────────────────────────────────
        decimal totalAmount = request.IsHourly
            ? Math.Ceiling((car.PricePerDay ?? 50m) / 10) * (request.DurationHours > 0 ? request.DurationHours : 1)
            : (car.PricePerDay ?? 50m) * Math.Max(1, (decimal)(dDate - pDate).TotalDays);

        // ── Build reservation ─────────────────────────────────────────────────
        var reservation = new Models.Reservation
        {
            UserId = userId,
            CarId = request.CarId,
            PickupLocationId = pickupLoc.LocationId,
            DropoffLocationId = dropoffLoc.LocationId,
            ReservationStatusId = 1, // Confirmed
            PickupDate = pDate,
            DropDate = dDate,
            TotalAmount = totalAmount,
            Address = request.Address,
            IsHourly = request.IsHourly,
            DurationHours = request.DurationHours,
            PickupTime = request.PickupTime,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // ── Payment validation & creation ─────────────────────────────────────
        var payMethod = await _context.PaymentMethods.FindAsync(request.PaymentMethodId)
            ?? await _context.PaymentMethods.FirstOrDefaultAsync()
            ?? new PaymentMethod { MethodName = "Credit Card" };

        if (payMethod.PaymentMethodId == 1 &&
            (string.IsNullOrWhiteSpace(request.CardNumber) ||
             string.IsNullOrWhiteSpace(request.ExpiryDate) ||
             string.IsNullOrWhiteSpace(request.Cvv)))
            return new BadRequestObjectResult(new { error = "Credit Card details (Card Number, Expiry, and CVV) are required." });

        if (payMethod.PaymentMethodId == 2 && string.IsNullOrWhiteSpace(request.PayPalEmail))
            return new BadRequestObjectResult(new { error = "PayPal Email is required to complete PayPal payment." });

        var payStatus = await _context.PaymentStatuses.FirstAsync(ps => ps.PaymentStatusId == 1);

        _context.Payments.Add(new Payment
        {
            ReservationId = reservation.ReservationId,
            PaymentMethodId = payMethod.PaymentMethodId,
            PaymentStatusId = payStatus.PaymentStatusId,
            TransactionId = $"TXN_{payMethod.MethodName.Replace(" ", "").ToUpper()}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Amount = reservation.TotalAmount,
            PaymentDate = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // ── Return mapped DTO ─────────────────────────────────────────────────
        var reloaded = await LoadReservationAsync(reservation.ReservationId);
        return _mapper.Map<ReservationDto>(reloaded);
    }

    // ── GET MY BOOKINGS ───────────────────────────────────────────────────────
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyBookings(int userId)
    {
        var reservations = await _context.Reservations
            .Include(r => r.PickupLocation)
            .Include(r => r.DropoffLocation)
            .Include(r => r.ReservationStatus)
            .Where(r => r.UserId == userId)
            .ToListAsync();

        return _mapper.Map<List<ReservationDto>>(reservations);
    }

    // ── GET ALL BOOKINGS ──────────────────────────────────────────────────────
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllBookingsAsync()
    {
        var reservations = await _context.Reservations
            .Include(r => r.PickupLocation)
            .Include(r => r.DropoffLocation)
            .Include(r => r.ReservationStatus)
            .ToListAsync();

        return _mapper.Map<List<ReservationDto>>(reservations);
    }

    // ── CANCEL ────────────────────────────────────────────────────────────────
    public async Task<ActionResult<bool>> CancelBooking(int id, int userId, bool isAdmin)
    {
        var res = await _context.Reservations.FindAsync(id);
        if (res == null) return new NotFoundObjectResult("Reservation not found.");

        if (!isAdmin && res.UserId != userId)
            throw new UnauthorizedAccessException("Unauthorized booking access.");

        res.ReservationStatusId = 3; // Cancelled
        res.UpdatedAt = DateTime.UtcNow;

        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.ReservationId == id);
        if (payment != null) payment.PaymentStatusId = 3; // Refunded

        await _context.SaveChangesAsync();
        return true;
    }

    // ── RETURN CAR ────────────────────────────────────────────────────────────
    public async Task<ActionResult<bool>> ReturnCarAsync(int id, int userId, bool isAdmin)
    {
        var res = await _context.Reservations
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.ReservationId == id);

        if (res == null) return new NotFoundObjectResult("Reservation not found.");

        if (!isAdmin && res.UserId != userId)
            throw new UnauthorizedAccessException("Unauthorized booking access.");

        if (res.ReservationStatusId == 3)
            throw new InvalidOperationException("Cannot return a cancelled booking.");

        res.ReservationStatusId = 2; // Completed
        res.UpdatedAt = DateTime.UtcNow;

        if (res.Car != null) res.Car.CarStatusId = 1; // Available

        await _context.SaveChangesAsync();
        return true;
    }

    // ── HELPER ────────────────────────────────────────────────────────────────
    private async Task<Models.Reservation> LoadReservationAsync(int id) =>
        await _context.Reservations
            .Include(r => r.PickupLocation)
            .Include(r => r.DropoffLocation)
            .Include(r => r.ReservationStatus)
            .FirstAsync(r => r.ReservationId == id);
}