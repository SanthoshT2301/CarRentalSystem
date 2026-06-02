namespace CarRentalSystem.DTO.Check;
public class GateCheckinRequest
    {
        public int MileageIn { get; set; }
        public int FuelIn { get; set; }
        public string? Damages { get; set; }
        public string AgentName { get; set; } = string.Empty;
    }
