namespace CarRentalSystem.DTO.Cars
{
    public class CreateCarRequest
    {
         public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal PricePerDay { get; set; }
        public decimal PricePerHour { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public int NoSeats { get; set; }
        public string Transmission { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<string> Features { get; set; } = new();
        public string Mileage { get; set; } = string.Empty;
    }
}
