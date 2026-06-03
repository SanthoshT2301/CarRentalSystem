using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Reservation;
using Microsoft.AspNetCore.Mvc;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;
namespace CarRentalSystem.Service.Reservation;

public class ReservationService : IReservationService
{
    private readonly AppDbContext _appDbContext;
    public ReservationService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    

    public async Task<ActionResult<ReservationDto>> CreateBooking(int userId,CreateReservationRequest request)
    {
        var car=await _appDbContext.Cars.FindAsync(request.CarId);
        if (car == null)
        {
            return new NotFoundObjectResult("Car not found");
        }
        var pickupLoc = await _appDbContext.Locations
    .FirstOrDefaultAsync(l => l.LocationName == request.PickupLocation);

if (pickupLoc == null)
{
    return new NotFoundObjectResult("Pickup location not found");
}

var dropoffLoc = await _appDbContext.Locations
    .FirstOrDefaultAsync(l => l.LocationName == request.DropoffLocation);

if (dropoffLoc == null)
{
    return new NotFoundObjectResult("Dropoff location not found");
}

DateTime.TryParse(request.PickupDate, out var pickupDate);
DateTime.TryParse(request.DropoffDate, out var dropoffDate);

var totalDays = (dropoffDate - pickupDate).Days;



if (totalDays <= 0)
{
    return new BadRequestObjectResult("Dropoff date must be after pickup date.");
}

var TotalAmount = car.PricePerDay * totalDays;


        var status=await _appDbContext.ReservationStatuses.FindAsync(1);
        DateTime pDate;
            DateTime dDate;

            if (request.IsHourly)
            {
                DateTime.TryParse(request.PickupDate, out var parsedDateOnly);
                if (parsedDateOnly == DateTime.MinValue)
                {
                    parsedDateOnly = DateTime.UtcNow.Date;
                }

                if (!string.IsNullOrEmpty(request.PickupTime) && TimeSpan.TryParse(request.PickupTime, out var pTime))
                {
                    pDate = parsedDateOnly.Date.Add(pTime);
                }
                else
                {
                    pDate = parsedDateOnly.Date.AddHours(12); 
                }

                int hours = request.DurationHours > 0 ? request.DurationHours : 1;
                dDate = pDate.AddHours(hours);
            }
            else
            {
                DateTime.TryParse(request.PickupDate, out pDate);
                DateTime.TryParse(request.DropoffDate, out dDate);

                if (pDate == DateTime.MinValue) pDate = DateTime.UtcNow;
                if (dDate == DateTime.MinValue) dDate = pDate.AddDays(3);
            }
             var isAlreadyBooked = await _appDbContext.Reservations
                .AnyAsync(r => r.CarId == request.CarId 
                    && r.ReservationStatusId == 1 // Active / Confirmed reservation
                    && ((pDate >= r.PickupDate && pDate < r.DropDate) 
                        || (dDate > r.PickupDate && dDate <= r.DropDate) 
                        || (pDate <= r.PickupDate && dDate >= r.DropDate)));

            if (isAlreadyBooked)
            {
                throw new InvalidOperationException("This vehicle is already booked for the requested period. Please choose another car or choose a different time window.");
            }
            var hourlyRateSymbolic = Math.Ceiling((car.PricePerDay ?? 50.00m) / 10);
            var computedTotal = request.IsHourly
                ? (hourlyRateSymbolic * (request.DurationHours > 0 ? request.DurationHours : 1))
                : (car.PricePerDay ?? 50.00m) * Math.Max(1, (decimal)(dDate - pDate).TotalDays);
        var reservation=new CarRentalSystem.Models.Reservation
        {
                UserId=userId,
                CarId = request.CarId,
                PickupLocationId = pickupLoc.LocationId,
                DropoffLocationId = dropoffLoc.LocationId,
                ReservationStatusId = status?.ReservationStatusId ?? 1,
                PickupDate = pickupDate ,
                DropDate = dropoffDate ,
                TotalAmount = car.PricePerDay * totalDays,
                Address = request.Address,
                IsHourly = request.IsHourly,
                DurationHours = request.DurationHours,
                PickupTime = request.PickupTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
        };
        _appDbContext.Reservations.Add(reservation);
        await _appDbContext.SaveChangesAsync();
        var payMethod = await _appDbContext.PaymentMethods.FindAsync(request.PaymentMethodId);
            if (payMethod == null)
            {
                payMethod = await _appDbContext.PaymentMethods.FirstOrDefaultAsync<PaymentMethod>() ?? new PaymentMethod { MethodName = "Credit Card" };
            }


            if (payMethod.PaymentMethodId == 1) // Credit Card
            {
                if (string.IsNullOrWhiteSpace(request.CardNumber) || string.IsNullOrWhiteSpace(request.ExpiryDate) || string.IsNullOrWhiteSpace(request.Cvv))
                {
                    return new BadRequestObjectResult(new { error = "Credit Card details (Card Number, Expiry, and CVV) are required to complete payment." });
                }
            }
            else if (payMethod.PaymentMethodId == 2) // PayPal
            {
                if (string.IsNullOrWhiteSpace(request.PayPalEmail))
                {
                    return new BadRequestObjectResult(new { error = "PayPal Email is required to complete PayPal payment." });
                }
            }

            var payStatus = await _appDbContext.PaymentStatuses.FirstOrDefaultAsync(ps => ps.PaymentStatusId == 1) ;
                            

            var payment = new Payment
            {
                ReservationId = reservation.ReservationId,
                PaymentMethodId = payMethod.PaymentMethodId,
                PaymentStatusId = payStatus.PaymentStatusId,
                TransactionId = "TXN_" + (payMethod.MethodName.Replace(" ", "").ToUpper()) + "_" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Amount = reservation.TotalAmount,
                PaymentDate = DateTime.UtcNow
            };

            _appDbContext.Payments.Add(payment);
            await _appDbContext.SaveChangesAsync();

            var reloaded = await _appDbContext.Reservations
                .Include(r => r.PickupLocation)
                .Include(r => r.DropoffLocation)
                .Include(r => r.ReservationStatus)
                .FirstAsync(r => r.ReservationId == reservation.ReservationId);

            var reservationDto=new ReservationDto
            {
            Id=reloaded.ReservationId,
            CarId=reloaded.CarId,
            UserId=reloaded.UserId,
            PickupLocation=reloaded.PickupLocation.LocationName,
            DropoffLocation=reloaded.DropoffLocation.LocationName,
            PickupDate=reloaded.PickupDate.ToString(),
            DropoffDate=reloaded.DropDate.ToString(),
            TotalAmount= (decimal)reloaded.TotalAmount,
            Address=reloaded.Address,
            Status=reloaded.ReservationStatus.StatusName
            };
        return reservationDto;
    }

    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyBookings(int userId)
{
     var myReservations = await _appDbContext.Reservations
                .Include(r => r.PickupLocation)
                .Include(r => r.DropoffLocation)
                .Include(r => r.ReservationStatus)
                .Where(r => r.UserId == userId)
                .ToListAsync();

            var reservationIds = myReservations.Select(r => r.ReservationId).ToList();
            var reviewsMap = await _appDbContext.Reviews
                .Where(rv => reservationIds.Contains(rv.ReservationId))
                .ToDictionaryAsync(rv => rv.ReservationId, rv => rv.ReviewId);

            var dtos = myReservations.Select(r => new ReservationDto
            {
                Id = r.ReservationId,
                CarId = r.CarId,
                UserId = r.UserId,
                PickupLocation = r.PickupLocation?.LocationName ?? "",
                DropoffLocation = r.DropoffLocation?.LocationName ?? "",
                PickupDate = r.PickupDate.ToString("yyyy-MM-dd"),
                DropoffDate = r.DropDate.ToString("yyyy-MM-dd"),
                TotalAmount = (decimal)r.TotalAmount,
                Address = r.Address ?? "",
                Status = r.ReservationStatus?.StatusName ?? "",
                IsHourly = r.IsHourly,
                DurationHours = r.DurationHours,
                PickupTime = r.PickupTime
            }).ToList();

            return dtos;
}
   public async Task<ActionResult<bool>> CancelBooking(int id,int userId,bool isAdmin)
{
    var res = await _appDbContext.Reservations.FindAsync(id);
            if (res == null)
            {
                return false;
            }

            if (!isAdmin && res.UserId != userId)
            {
                throw new UnauthorizedAccessException("Unauthorized booking access.");
            }

            res.ReservationStatusId = 3; 
            res.UpdatedAt = DateTime.UtcNow;

            var payment = await _appDbContext.Payments.FirstOrDefaultAsync(p => p.ReservationId == id);
            if (payment != null)
            {
                payment.PaymentStatusId = 3; 
            }

            await _appDbContext.SaveChangesAsync();
            return true;
}

    public async Task<ActionResult<bool>> ReturnCarAsync(int id, int userId, bool isAdmin)
    {
       var res = await _appDbContext.Reservations
                .Include(r => r.Car)
                .Include(r => r.ReservationStatus)
                .FirstOrDefaultAsync(r => r.ReservationId == id);

            if (res == null)
            {
                return false;
            }

            if (!isAdmin && res.UserId != userId)
            {
                throw new UnauthorizedAccessException("Unauthorized booking access.");
            }

            if (res.ReservationStatusId == 3) 
            {
                throw new InvalidOperationException("Cannot return a cancelled booking.");
            }

            res.ReservationStatusId = 2; // Completed
            res.UpdatedAt = DateTime.UtcNow;
            if (res.Car != null)
            {
                res.Car.CarStatusId = 1; // Available
            }
            await _appDbContext.SaveChangesAsync();
            return true;
    }

    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllBookingsAsync()
    {
         var reservations = await _appDbContext.Reservations
                .Include(r => r.PickupLocation)
                .Include(r => r.DropoffLocation)
                .Include(r => r.ReservationStatus)
                .ToListAsync();

            var reservationIds = reservations.Select(r => r.ReservationId).ToList();
            var reviewsMap = await _appDbContext.Reviews
                .Where(rv => reservationIds.Contains(rv.ReservationId))
                .ToDictionaryAsync(rv => rv.ReservationId, rv => rv.ReviewId);

            var dtos = reservations.Select(r => new ReservationDto
            {
                Id = r.ReservationId,
                CarId = r.CarId,
                UserId = r.UserId,
                PickupLocation = r.PickupLocation?.LocationName ?? "",
                DropoffLocation = r.DropoffLocation?.LocationName ?? "",
                PickupDate = r.PickupDate.ToString("yyyy-MM-dd"),
                DropoffDate = r.DropDate.ToString("yyyy-MM-dd"),
                TotalAmount = (decimal)r.TotalAmount,
                Address = r.Address ?? "",
                Status = r.ReservationStatus?.StatusName ?? "",
                IsHourly = r.IsHourly,
                DurationHours = r.DurationHours,
                PickupTime = r.PickupTime
            }).ToList();

            return dtos;
    }
}