namespace CarRentalSystem.DTO.Reports
{
    public class ReviewReportDto
    {
        public int ReviewId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CarName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public bool IsDisputed { get; set; }
        public string DisputeResolution { get; set; } = string.Empty;
        public string CreatedAt { get; set; } = string.Empty;
    }

}
