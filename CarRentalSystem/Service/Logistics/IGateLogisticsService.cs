using CarRentalSystem.DTO.Check;
using CarRentalSystem.DTO.Common;
using CarRentalSystem.DTO.Reservation;

namespace CarRentalSystem.Service.Logistics
{
    public interface IGateLogisticsService
    {
        Task<CheckoutDetailsDto> VerifyAndCheckoutAsync(int reservationId, GateCheckoutRequest request);
        Task<CheckinDetailsDto> VerifyAndCheckinAsync(int reservationId, GateCheckinRequest request);
        Task<CheckoutDetailsDto?> GetCheckoutDetailsByReservationAsync(int reservationId);
        Task<CheckinDetailsDto?> GetCheckinDetailsByReservationAsync(int reservationId);

        /// <summary>
        /// Returns only the bookings whose Car was added by the given agent.
        /// </summary>
        Task<PagedResult<ReservationDto>> GetBookingsForAgentCarsAsync(int agentId, int page, int pageSize);
    }
}