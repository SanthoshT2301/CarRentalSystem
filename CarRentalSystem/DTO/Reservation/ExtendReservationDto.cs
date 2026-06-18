namespace CarRentalSystem.DTO.Reservation;

public class ExtendReservationDto
{
    public int ReservationId { get; set; }
    public string OldDropoffDate { get; set; } = string.Empty;
    public string NewDropoffDate { get; set; } = string.Empty;
    public decimal ExtraCharge { get; set; }
    public decimal NewTotalAmount { get; set; }
    public string Message { get; set; } = string.Empty;
}