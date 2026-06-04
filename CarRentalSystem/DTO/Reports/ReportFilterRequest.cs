namespace CarRentalSystem.DTO.Reports
{
    public class ReportFilterRequest
    {
        public string? StartDate { get; set; }   // "yyyy-MM-dd"
        public string? EndDate { get; set; }   // "yyyy-MM-dd"
    }
}
