namespace CarRentalSystem.DTO.Reservation;

public class ExtendReservationRequest
{
    public string? NewDropoffDate { get; set; } = null;
    public int? AdditionalHours { get; set; }
}