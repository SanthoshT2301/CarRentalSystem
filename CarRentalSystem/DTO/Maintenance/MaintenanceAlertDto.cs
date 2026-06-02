namespace CarRentalSystem.DTO.Maintenance;
 public class MaintenanceAlertDto
    {
        public int MaintenanceAlertId { get; set; }
        public int CarId { get; set; }
        public string CarName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Priority { get; set; } = "Medium";
        public string ReportedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Reported";
        public DateTime CreatedAt { get; set; }
    }
