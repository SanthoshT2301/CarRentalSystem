using AutoMapper;
using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Check;
using CarRentalSystem.DTO.Common;
using CarRentalSystem.DTO.Reservation;
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

        // ── Get checkin / checkout details ────────────────────────────────────

        public async Task<CheckinDetailsDto?> GetCheckinDetailsByReservationAsync(int reservationId)
        {
            var cid = await _context.CheckinDetails
                .FirstOrDefaultAsync(c => c.ReservationId == reservationId);
            return cid == null ? null : _mapper.Map<CheckinDetailsDto>(cid);
        }

        public async Task<CheckoutDetailsDto?> GetCheckoutDetailsByReservationAsync(int reservationId)
        {
            var cod = await _context.CheckoutDetails
                .FirstOrDefaultAsync(c => c.ReservationId == reservationId);
            return cod == null ? null : _mapper.Map<CheckoutDetailsDto>(cod);
        }

        // ── Bookings scoped to an agent's cars ────────────────────────────────

        /// <summary>
        /// Returns all reservations where the booked car was added by <paramref name="agentId"/>.
        /// Only confirmed reservations are included (i.e. the ones relevant to gate ops).
        /// </summary>
        public async Task<PagedResult<ReservationDto>> GetBookingsForAgentCarsAsync(
            int agentId, int page, int pageSize)
        {
            var query = _context.Reservations
                .Include(r => r.Car)
                .Include(r => r.PickupLocation)
                .Include(r => r.DropoffLocation)
                .Include(r => r.ReservationStatus)
                .Where(r => r.Car != null && r.Car.AgentId == agentId)
                .OrderByDescending(r => r.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ReservationDto>
            {
                Data = _mapper.Map<List<ReservationDto>>(items),
                Page = page,
                PageSize = pageSize,
                TotalCount = total
            };
        }

        // ── Checkout ──────────────────────────────────────────────────────────

        public async Task<CheckoutDetailsDto> VerifyAndCheckoutAsync(
            int reservationId, GateCheckoutRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var res = await _context.Reservations
                .Include(r => r.Car)
                .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

            if (res == null) throw new ArgumentException("Reservation not found.");

            var checkout = _mapper.Map<CheckoutDetails>(request);
            checkout.ReservationId = reservationId;
            checkout.CompletedAt = DateTime.UtcNow;

            _context.CheckoutDetails.Add(checkout);

            if (res.Car != null) res.Car.CarStatusId = 2; // Rented

            await _context.SaveChangesAsync();
            return _mapper.Map<CheckoutDetailsDto>(checkout);
        }

        // ── Check-in ─────────────────────────────────────────────────────────

        public async Task<CheckinDetailsDto> VerifyAndCheckinAsync(
            int reservationId, GateCheckinRequest request)
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

            res.ReservationStatusId = 2; // Completed
            res.UpdatedAt = DateTime.UtcNow;

            bool hasDamages = !string.IsNullOrWhiteSpace(request.Damages) &&
                              !request.Damages.Trim().Equals("none", StringComparison.OrdinalIgnoreCase);
            if (res.Car != null)
                res.Car.CarStatusId = hasDamages ? 4 : 1; // 4=Clean-up Required, 1=Available

            await _context.SaveChangesAsync();
            return _mapper.Map<CheckinDetailsDto>(checkin);
        }
    }
}