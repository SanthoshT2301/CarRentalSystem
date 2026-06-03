using CarRentalSystem.DTO.Check;

namespace CarRentalSystem.Service.Logistics
{
      public interface IGateLogisticsService
    {
        Task<CheckoutDetailsDto> VerifyAndCheckoutAsync(int reservationId, GateCheckoutRequest request);
        Task<CheckinDetailsDto> VerifyAndCheckinAsync(int reservationId, GateCheckinRequest request);
        Task<CheckoutDetailsDto?> GetCheckoutDetailsByReservationAsync(int reservationId);
        Task<CheckinDetailsDto?> GetCheckinDetailsByReservationAsync(int reservationId);
    }
}