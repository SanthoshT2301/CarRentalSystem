namespace CarRentalSystem.DTO.Reports
{
    public class RevenueReportDto
    {
        public string Period { get; set; } = string.Empty;   // "YYYY-MM"
        public int TotalBookings { get; set; }
        public int CompletedRentals { get; set; }
        public int CancelledRentals { get; set; }
        public decimal GrossRevenue { get; set; }
        public decimal Refunds { get; set; }
        public decimal NetRevenue { get; set; }
        public string TopEarningCar { get; set; } = string.Empty;
    }

}