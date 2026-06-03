using AutoMapper;
using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Check;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Logistics
{
    public class GateLogisticsService : IGateLogisticsService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public GateLogisticsService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<CheckinDetailsDto?> GetCheckinDetailsByReservationAsync(int reservationId)
        {
             var cid = await _context.CheckinDetails
                .FirstOrDefaultAsync(c => c.ReservationId == reservationId);

            if (cid == null) return null;
            return _mapper.Map<CheckinDetailsDto>(cid);
        }

        public async Task<CheckoutDetailsDto?> GetCheckoutDetailsByReservationAsync(int reservationId)
        {
            var cod = await _context.CheckoutDetails
                .FirstOrDefaultAsync(c => c.ReservationId == reservationId);

            if (cod == null) return null;
            return _mapper.Map<CheckoutDetailsDto>(cod);
        }

        public async Task<CheckinDetailsDto> VerifyAndCheckinAsync(int reservationId, GateCheckinRequest request)
        {
             if (request == null) throw new ArgumentNullException(nameof(request));

            var res = await _context.Reservations
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (res == null) throw new ArgumentException("Reservation not found.");

           
            var checkin = _mapper.Map<CheckinDetails>(request);
            checkin.ReservationId = reservationId;
            checkin.CompletedAt = DateTime.UtcNow;

            _context.CheckinDetails.Add(checkin);

            res.ReservationStatusId = 2;
            res.UpdatedAt = DateTime.UtcNow;

            // Determine if vehicle requires turnaround or is instantly ready
            bool hasDamages = !string.IsNullOrWhiteSpace(request.Damages) && !request.Damages.Trim().Equals("none", StringComparison.OrdinalIgnoreCase);
            if (res.Car != null)
            {
                res.Car.CarStatusId = hasDamages ? 4 : 1; // 4 = "Clean-up Required", 1 = "Available"
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<CheckinDetailsDto>(checkin);
        }

        public async Task<CheckoutDetailsDto> VerifyAndCheckoutAsync(int reservationId, GateCheckoutRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var res = await _context.Reservations
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (res == null) throw new ArgumentException("Reservation not found.");

            // Create checkout details entry
            var checkout = _mapper.Map<CheckoutDetails>(request);
            checkout.ReservationId = reservationId;
            checkout.CompletedAt = DateTime.UtcNow;

            _context.CheckoutDetails.Add(checkout);

            // Update car status to "Rented" (StatusId = 2)
            if (res.Car != null)
            {
                res.Car.CarStatusId = 2;
            }

            await _context.SaveChangesAsync();
            return _mapper.Map<CheckoutDetailsDto>(checkout);
        }
    }
}