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

    public async Task<ActionResult<ReservationDto>> CreateBooking(int userId, CreateReservationRequest request)
    {
        var car = await _context.Cars.FindAsync(request.CarId);
        if (car == null) return new NotFoundObjectResult("Car not found.");

        var pickupLoc = await _context.Locations.FirstOrDefaultAsync(l => l.LocationName == request.PickupLocation);
        if (pickupLoc == null) return new NotFoundObjectResult("Pickup location not found.");

        var dropoffLoc = await _context.Locations.FirstOrDefaultAsync(l => l.LocationName == request.DropoffLocation);
        if (dropoffLoc == null) return new NotFoundObjectResult("Dropoff location not found.");

        DateTime pDate, dDate;
        if (request.IsHourly)
        {
            DateTime.TryParse(request.PickupDate, out var parsedDate);
            if (parsedDate == DateTime.MinValue) parsedDate = DateTime.UtcNow.Date;
            pDate = TimeSpan.TryParse(request.PickupTime, out var pTime) ? parsedDate.Date.Add(pTime) : parsedDate.Date.AddHours(12);
            dDate = pDate.AddHours(request.DurationHours > 0 ? request.DurationHours : 1);
        }
        else
        {
            DateTime.TryParse(request.PickupDate, out pDate);
            DateTime.TryParse(request.DropoffDate, out dDate);
            if (pDate == DateTime.MinValue) pDate = DateTime.UtcNow;
            if (dDate == DateTime.MinValue) dDate = pDate.AddDays(3);
            if ((dDate - pDate).Days <= 0) return new BadRequestObjectResult("Dropoff date must be after pickup date.");
        }

        var isBooked = await _context.Reservations.AnyAsync(r =>
            r.CarId == request.CarId && r.ReservationStatusId == 1 &&
            ((pDate >= r.PickupDate && pDate < r.DropDate) ||
             (dDate > r.PickupDate && dDate <= r.DropDate) ||
             (pDate <= r.PickupDate && dDate >= r.DropDate)));
        if (isBooked)
            throw new InvalidOperationException("This vehicle is already booked for the requested period.");

        decimal totalAmount = request.IsHourly
            ? Math.Ceiling((car.PricePerDay ?? 50m) / 10) * (request.DurationHours > 0 ? request.DurationHours : 1)
            : (car.PricePerDay ?? 50m) * Math.Max(1, (decimal)(dDate - pDate).TotalDays);

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
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        var payMethod = await _context.PaymentMethods.FindAsync(request.PaymentMethodId)
            ?? await _context.PaymentMethods.FirstOrDefaultAsync()
            ?? new PaymentMethod { MethodName = "Credit Card" };

        if (payMethod.PaymentMethodId == 1 &&
            (string.IsNullOrWhiteSpace(request.CardNumber) || string.IsNullOrWhiteSpace(request.ExpiryDate) || string.IsNullOrWhiteSpace(request.Cvv)))
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

    // paginated
    public async Task<PagedResult<ReservationDto>> GetMyBookings(int userId, int page, int pageSize)
    {
        var query = _context.Reservations
            .Include(r => r.PickupLocation).Include(r => r.DropoffLocation).Include(r => r.ReservationStatus)
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

    // paginated
    public async Task<PagedResult<ReservationDto>> GetAllBookingsAsync(int page, int pageSize)
    {
        var query = _context.Reservations
            .Include(r => r.PickupLocation).Include(r => r.DropoffLocation).Include(r => r.ReservationStatus)
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

    public async Task<ActionResult<bool>> CancelBooking(int id, int userId, bool isAdmin)
    {
        var res = await _context.Reservations.FindAsync(id);
        if (res == null) return new NotFoundObjectResult("Reservation not found.");
        if (!isAdmin && res.UserId != userId) throw new UnauthorizedAccessException("Unauthorized booking access.");
        res.ReservationStatusId = 3;
        res.UpdatedAt = DateTime.UtcNow;
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.ReservationId == id);
        if (payment != null) payment.PaymentStatusId = 3;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ActionResult<bool>> ReturnCarAsync(int id, int userId, bool isAdmin)
    {
        var res = await _context.Reservations.Include(r => r.Car).FirstOrDefaultAsync(r => r.ReservationId == id);
        if (res == null) return new NotFoundObjectResult("Reservation not found.");
        if (!isAdmin && res.UserId != userId) throw new UnauthorizedAccessException("Unauthorized booking access.");
        if (res.ReservationStatusId == 3) throw new InvalidOperationException("Cannot return a cancelled booking.");
        res.ReservationStatusId = 2;
        res.UpdatedAt = DateTime.UtcNow;
        if (res.Car != null) res.Car.CarStatusId = 1;
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task<Models.Reservation> LoadReservationAsync(int id) =>
        await _context.Reservations
            .Include(r => r.PickupLocation).Include(r => r.DropoffLocation).Include(r => r.ReservationStatus)
            .FirstAsync(r => r.ReservationId == id);
}