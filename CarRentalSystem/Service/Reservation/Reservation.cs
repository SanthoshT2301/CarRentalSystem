using AutoMapper;
using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Common;
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

    // ── CREATE ────────────────────────────────────────────────────────────────
    public async Task<ActionResult<ReservationDto>> CreateBooking(
        int userId, CreateReservationRequest request)
    {
        var car = await _context.Cars.FindAsync(request.CarId);
        if (car == null) return new NotFoundObjectResult("Car not found.");

        var pickupLoc = await _context.Locations
            .FirstOrDefaultAsync(l => l.LocationName == request.PickupLocation);
        if (pickupLoc == null) return new NotFoundObjectResult("Pickup location not found.");

        var dropoffLoc = await _context.Locations
            .FirstOrDefaultAsync(l => l.LocationName == request.DropoffLocation);
        if (dropoffLoc == null) return new NotFoundObjectResult("Dropoff location not found.");

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

        var isBooked = await _context.Reservations.AnyAsync(r =>
            r.CarId == request.CarId && r.ReservationStatusId == 1 &&
            ((pDate >= r.PickupDate && pDate < r.DropDate) ||
             (dDate > r.PickupDate && dDate <= r.DropDate) ||
             (pDate <= r.PickupDate && dDate >= r.DropDate)));
        if (isBooked)
            throw new InvalidOperationException(
                "This vehicle is already booked for the requested period.");

        decimal totalAmount = request.IsHourly
            ? Math.Ceiling((car.PricePerDay ?? 50m) / 10) *
              (request.DurationHours > 0 ? request.DurationHours : 1)
            : (car.PricePerDay ?? 50m) *
              Math.Max(1, (decimal)(dDate - pDate).TotalDays);

        var reservation = new Models.Reservation
        {
            UserId = userId,
            CarId = request.CarId,
            PickupLocationId = pickupLoc.LocationId,
            DropoffLocationId = dropoffLoc.LocationId,
            ReservationStatusId = 1,
            PickupDate = pDate,
            DropDate = dDate,
            TotalAmount = totalAmount,
            Address = request.Address,
            IsHourly = request.IsHourly,
            DurationHours = request.DurationHours,
            PickupTime = request.PickupTime,
            IsExtended = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var payMethod = await _context.PaymentMethods.FindAsync(request.PaymentMethodId)
            ?? await _context.PaymentMethods.FirstOrDefaultAsync()
            ?? new PaymentMethod { MethodName = "Credit Card" };

        if (payMethod.PaymentMethodId == 1 &&
            (string.IsNullOrWhiteSpace(request.CardNumber) ||
             string.IsNullOrWhiteSpace(request.ExpiryDate) ||
             string.IsNullOrWhiteSpace(request.Cvv)))
            return new BadRequestObjectResult(new { error = "Credit Card details are required." });

        if (payMethod.PaymentMethodId == 2 && string.IsNullOrWhiteSpace(request.PayPalEmail))
            return new BadRequestObjectResult(new { error = "PayPal Email is required." });

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

        var reloaded = await LoadReservationAsync(reservation.ReservationId);
        return _mapper.Map<ReservationDto>(reloaded);
    }

    // ── EXTEND (one-time, daily only) ─────────────────────────────────────────
   

    public async Task<ActionResult<ExtendReservationDto>> ExtendReservationAsync(
        int reservationId, int userId, ExtendReservationRequest request)
    {
        var res = await _context.Reservations
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

        if (res == null)
            return new NotFoundObjectResult("Reservation not found.");

        if (res.UserId != userId)
            return new BadRequestObjectResult("You can only extend your own reservations.");

        if (res.ReservationStatusId != 1)
            return new BadRequestObjectResult("Only confirmed reservations can be extended.");

        if (res.IsExtended)
            return new BadRequestObjectResult(
                "This reservation has already been extended once. No further extensions are allowed.");

        var pricePerDay = res.Car?.PricePerDay ?? 50m;
        var oldDropDate = res.DropDate;
        var oldTotalAmount = res.TotalAmount ?? 0m;

        DateTime newDropDate;
        decimal extraCharge;
        string durationDescription;

        // ── HOURLY path ───────────────────────────────────────────────────────
        if (res.IsHourly)
        {
            int additionalHours = request.AdditionalHours ?? 0;
            if (additionalHours <= 0)
                return new BadRequestObjectResult("Additional hours must be at least 1.");

            if (additionalHours > 24)
                return new BadRequestObjectResult("Cannot extend by more than 24 hours at a time.");

            // Total duration after extension
            var currentDuration = (res.DropDate - res.PickupDate).TotalHours;
            if (currentDuration + additionalHours > 24)
                return new BadRequestObjectResult(
                    $"Total rental duration cannot exceed 24 hours. " +
                    $"Current duration: {(int)currentDuration}h. Max additional: {24 - (int)currentDuration}h.");

            newDropDate = res.DropDate.AddHours(additionalHours);

            // Conflict check
            var hasConflict = await _context.Reservations.AnyAsync(r =>
                r.CarId == res.CarId &&
                r.ReservationId != reservationId &&
                r.ReservationStatusId == 1 &&
                r.PickupDate < newDropDate &&
                r.DropDate > res.DropDate);

            if (hasConflict)
                return new BadRequestObjectResult(
                    "The car is already booked during the extended period. Please choose fewer hours.");

            var pricePerHour = Math.Ceiling(pricePerDay / 10m);
            extraCharge = Math.Round(pricePerHour * additionalHours, 2);
            durationDescription = $"{additionalHours} hour{(additionalHours == 1 ? "" : "s")}";

            // Update DurationHours to reflect new total
            res.DurationHours = (int)(newDropDate - res.PickupDate).TotalHours;
        }
        // ── DAILY path ────────────────────────────────────────────────────────
        else
        {
            if (string.IsNullOrWhiteSpace(request.NewDropoffDate))
                return new BadRequestObjectResult("NewDropoffDate is required for daily bookings.");

            if (!DateTime.TryParse(request.NewDropoffDate, out newDropDate))
                return new BadRequestObjectResult("Invalid date format. Use yyyy-MM-dd.");

            newDropDate = newDropDate.Date;

            if (newDropDate <= res.DropDate.Date)
                return new BadRequestObjectResult(
                    $"New drop-off date must be after the current drop-off date ({res.DropDate:yyyy-MM-dd}).");

            var hasConflict = await _context.Reservations.AnyAsync(r =>
                r.CarId == res.CarId &&
                r.ReservationId != reservationId &&
                r.ReservationStatusId == 1 &&
                r.PickupDate < newDropDate &&
                r.DropDate > res.DropDate);

            if (hasConflict)
                return new BadRequestObjectResult(
                    "The car is already booked by another customer during the extended period. Please choose an earlier date.");

            var extraDays = (decimal)(newDropDate - res.DropDate.Date).TotalDays;
            extraCharge = Math.Round(pricePerDay * extraDays, 2);
            durationDescription = $"{(int)extraDays} day{((int)extraDays == 1 ? "" : "s")}";
        }

        res.DropDate = newDropDate;
        res.TotalAmount = oldTotalAmount + extraCharge;
        res.IsExtended = true;
        res.UpdatedAt = DateTime.UtcNow;

        var payMethod = await _context.Payments
            .Where(p => p.ReservationId == reservationId)
            .Select(p => p.PaymentMethodId)
            .FirstOrDefaultAsync();

        _context.Payments.Add(new Payment
        {
            ReservationId = reservationId,
            PaymentMethodId = payMethod == 0 ? 1 : payMethod,
            PaymentStatusId = 1,
            TransactionId = $"TXN_EXTEND_{reservationId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}",
            Amount = extraCharge,
            PaymentDate = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return new ExtendReservationDto
        {
            ReservationId = reservationId,
            OldDropoffDate = oldDropDate.ToString(res.IsHourly ? "yyyy-MM-dd HH:mm" : "yyyy-MM-dd"),
            NewDropoffDate = newDropDate.ToString(res.IsHourly ? "yyyy-MM-dd HH:mm" : "yyyy-MM-dd"),
            ExtraCharge = extraCharge,
            NewTotalAmount = res.TotalAmount ?? 0m,
            Message = $"Reservation extended by {durationDescription}. Extra charge of ${extraCharge} has been processed."
        };
    }

    // ── GET MY BOOKINGS (paginated) ───────────────────────────────────────────
    public async Task<PagedResult<ReservationDto>> GetMyBookings(
        int userId, int page, int pageSize)
    {
        var query = _context.Reservations
            .Include(r => r.PickupLocation)
            .Include(r => r.DropoffLocation)
            .Include(r => r.ReservationStatus)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Data = _mapper.Map<List<ReservationDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    // ── GET ALL BOOKINGS (paginated) ──────────────────────────────────────────
    public async Task<PagedResult<ReservationDto>> GetAllBookingsAsync(int page, int pageSize)
    {
        var query = _context.Reservations
            .Include(r => r.PickupLocation)
            .Include(r => r.DropoffLocation)
            .Include(r => r.ReservationStatus)
            .OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReservationDto>
        {
            Data = _mapper.Map<List<ReservationDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
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

    // ── RETURN ────────────────────────────────────────────────────────────────
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

    // ── Helpers ───────────────────────────────────────────────────────────────
    private async Task<Models.Reservation> LoadReservationAsync(int id) =>
        await _context.Reservations
            .Include(r => r.PickupLocation)
            .Include(r => r.DropoffLocation)
            .Include(r => r.ReservationStatus)
            .FirstAsync(r => r.ReservationId == id);
}