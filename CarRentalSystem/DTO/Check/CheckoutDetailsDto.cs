namespace CarRentalSystem.DTO.Check;
public class CheckoutDetailsDto
    {
        public int CheckoutDetailsId { get; set; }
        public int ReservationId { get; set; }
        public string DriverLicense { get; set; } = string.Empty;
        public int MileageOut { get; set; }
        public int FuelOut { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
    }