using CarRentalSystem.DTO.Maintenance;

namespace CarRentalSystem.Service.Maintenances;
 public interface IMaintenanceService
    {
        Task<List<MaintenanceAlertDto>> GetMaintenanceAlertsAsync();
        Task<MaintenanceAlertDto> AddMaintenanceAlertAsync(CreateMaintenanceAlertRequest request);
        Task<MaintenanceAlertDto> UpdateMaintenanceAlertStatusAsync(int alertId, string status);
    }