namespace CarRentalSystem.DTO.Check;
 public class GateCheckoutRequest
    {
        public string DriverLicense { get; set; } = string.Empty;
        public int MileageOut { get; set; }
        public int FuelOut { get; set; }
        public string AgentName { get; set; } = string.Empty;
    }