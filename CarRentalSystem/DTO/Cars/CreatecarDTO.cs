namespace CarRentalSystem.DTO.Cars
{
    public class CreatecarDTO
    {
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public int FuelTypeId { get; set; }
        public int CarStatusId { get; set; }
        public int LocationId { get; set; }
        public string Model { get; set; } = string.Empty;
        public int? CarYear { get; set; }
        public string? Color { get; set; }
        public int? NoSeats { get; set; }
        public string? Transmission { get; set; }
        public string? Mileage { get; set; }
        public decimal? PricePerDay { get; set; }
        public string? Address { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
