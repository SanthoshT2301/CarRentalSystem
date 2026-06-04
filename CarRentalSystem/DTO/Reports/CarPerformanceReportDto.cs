namespace CarRentalSystem.DTO.Reports
{
    public class CarPerformanceReportDto
    {
        public int CarId { get; set; }
        public string CarName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int TotalBookings { get; set; }
        public int CompletedRentals { get; set; }
        public int CancelledRentals { get; set; }
        public decimal TotalRevenue { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public double UtilizationRate { get; set; }  // % of days rented
    }

}
