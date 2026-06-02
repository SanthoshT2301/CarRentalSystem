 namespace CarRentalSystem.DTO.Maintenance;
 public class CreateMaintenanceAlertRequest
    {
        public int CarId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string ReportedBy { get; set; } = string.Empty;
    }