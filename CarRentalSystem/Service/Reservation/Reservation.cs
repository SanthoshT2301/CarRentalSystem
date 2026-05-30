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

    

    public async Task<ActionResult<ReservationDto>> CreateBooking(CreateReservationRequest request)
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
        
        var reservation=new CarRentalSystem.Models.Reservation
        {
                UserId=request.UserId,
                CarId = request.CarId,
                PickupLocationId = pickupLoc.LocationId,
                DropoffLocationId = dropoffLoc.LocationId,
                ReservationStatusId = status?.ReservationStatusId ?? 1,
                PickupDate = pickupDate ,
                DropDate = dropoffDate ,
                TotalAmount = car.PricePerDay * totalDays,
                Address = request.Address,
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

    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetMyBookings()
{
    var myReservations = await _appDbContext.Reservations
        .Include(r => r.PickupLocation)
        .Include(r => r.DropoffLocation)
        .Include(r => r.ReservationStatus)
        .ToListAsync();

    if (!myReservations.Any())
    {
        return new NotFoundObjectResult("No reservations found");
    }

    var reservationsDto = myReservations.Select(reservation => new ReservationDto
    {
        Id = reservation.ReservationId,
        CarId = reservation.CarId,
        UserId = reservation.UserId,
        PickupLocation = reservation.PickupLocation?.LocationName ?? string.Empty,
        DropoffLocation = reservation.DropoffLocation?.LocationName ?? string.Empty,
        PickupDate = reservation.PickupDate.ToString("yyyy-MM-dd"),
        DropoffDate = reservation.DropDate.ToString("yyyy-MM-dd"),
        TotalAmount = (decimal)reservation.TotalAmount,
        Address = reservation.Address ?? string.Empty,
        Status = reservation.ReservationStatus?.StatusName ?? string.Empty
    }).ToList();

    return reservationsDto;
}
   public async Task<ActionResult<ReservationDto>> CancelBooking(int id)
{
    var reservation = await _appDbContext.Reservations
        .FirstOrDefaultAsync(r => r.ReservationId == id);

    if (reservation == null)
    {
        return new NotFoundObjectResult("No reservation with that ID");
    }

    reservation.ReservationStatusId = 3;
    reservation.UpdatedAt = DateTime.UtcNow;

    var payment = await _appDbContext.Payments
        .FirstOrDefaultAsync(p => p.ReservationId == id);

    if (payment != null)
    {
        payment.PaymentStatusId = 3;
    }

    await _appDbContext.SaveChangesAsync();

    var reloaded = await _appDbContext.Reservations
        .Include(r => r.PickupLocation)
        .Include(r => r.DropoffLocation)
        .Include(r => r.ReservationStatus)
        .FirstOrDefaultAsync(r => r.ReservationId == id);

    if (reloaded == null)
    {
        return new NotFoundObjectResult("Reservation not found");
    }

    return new ReservationDto
    {
        Id = reloaded.ReservationId,
        CarId = reloaded.CarId,
        UserId = reloaded.UserId,
        PickupLocation = reloaded.PickupLocation?.LocationName ?? string.Empty,
        DropoffLocation = reloaded.DropoffLocation?.LocationName ?? string.Empty,
        PickupDate = reloaded.PickupDate.ToString("yyyy-MM-dd"),
        DropoffDate = reloaded.DropDate.ToString("yyyy-MM-dd"),
        TotalAmount = (decimal)reloaded.TotalAmount,
        Address = reloaded.Address ?? string.Empty,
        Status = reloaded.ReservationStatus?.StatusName ?? string.Empty
    };
}
}