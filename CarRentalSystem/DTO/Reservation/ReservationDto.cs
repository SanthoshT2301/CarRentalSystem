    namespace CarRentalSystem.DTO.Reservation;
 public class ReservationDto
    {
    public int Id { get; set; }
        public int CarId { get; set; }
        public int UserId { get; set; }
        public string PickupLocation { get; set; } = string.Empty;
        public string DropoffLocation { get; set; } = string.Empty;
        public string PickupDate { get; set; } = string.Empty;
        public string DropoffDate { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = "confirmed";
        public bool IsHourly { get; set; }
        public int DurationHours { get; set; }
        public string? PickupTime { get; set; }
    public bool IsExtended { get; set; }
}
