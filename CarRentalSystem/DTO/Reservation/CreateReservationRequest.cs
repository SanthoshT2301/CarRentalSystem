namespace CarRentalSystem.DTO.Reservation;
 public class CreateReservationRequest
    {
        public int CarId { get; set; }
        public string PickupLocation { get; set; } = string.Empty;
        public string DropoffLocation { get; set; } = string.Empty;
        public string PickupDate { get; set; } = string.Empty;
        public string DropoffDate { get; set; } = string.Empty; 
        public decimal TotalAmount { get; set; }
        public string Address { get; set; } = string.Empty;
        
        public int PaymentMethodId { get; set; } = 1; // Default to 1 (Credit Card)
        public string? CardNumber { get; set; }
        public string? ExpiryDate { get; set; }
        public string? Cvv { get; set; }
        public string? PayPalEmail { get; set; }
    }