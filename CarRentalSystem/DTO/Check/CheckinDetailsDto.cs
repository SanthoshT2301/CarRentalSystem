namespace CarRentalSystem.DTO.Check;
 public class CheckinDetailsDto
    {
        public int CheckinDetailsId { get; set; }
        public int ReservationId { get; set; }
        public int MileageIn { get; set; }
        public int FuelIn { get; set; }
        public string? Damages { get; set; }
        public string AgentName { get; set; } = string.Empty;
        public DateTime CompletedAt { get; set; }
    }