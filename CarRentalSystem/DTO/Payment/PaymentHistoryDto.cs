namespace CarRentalSystem.DTO.Payment;

public class PaymentHistoryDto
{
    public int PaymentId { get; set; }
    public int ReservationId { get; set; }
    public string CarName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    public string PaymentDate { get; set; } = string.Empty;
}