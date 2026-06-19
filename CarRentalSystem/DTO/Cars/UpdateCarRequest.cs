namespace CarRentalSystem.DTO.Cars
{
    public class UpdateCarRequest
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public decimal? PricePerDay { get; set; }
        public decimal? PricePerHour { get; set; }
    }
}