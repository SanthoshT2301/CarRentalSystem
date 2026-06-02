using AutoMapper;
using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Maintenance;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Maintenances;

public class MaintenanceService : IMaintenanceService
{
    private readonly AppDbContext _appDbContext;
    private readonly IMapper _mapper;
    public MaintenanceService(AppDbContext appDbContext,IMapper mapper)
    {
        _appDbContext = appDbContext;
        _mapper=mapper;
    }
    public async Task<MaintenanceAlertDto> AddMaintenanceAlertAsync(CreateMaintenanceAlertRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

            var car = await _appDbContext.Cars
                .Include(c => c.Brand)
                .FirstOrDefaultAsync(c => c.CarId == request.CarId);
            if (car == null) throw new ArgumentException("Vehicle not found.");
            var alert = _mapper.Map<MaintenanceAlert>(request);
            alert.Status = "Reported";
            alert.CreatedAt = DateTime.UtcNow;
            _appDbContext.MaintenanceAlerts.Add(alert);
            car.CarStatusId = 3;
            await _appDbContext.SaveChangesAsync();
            alert.Car = car;
            return _mapper.Map<MaintenanceAlertDto>(alert);
        }

    public async Task<List<MaintenanceAlertDto>> GetMaintenanceAlertsAsync()
    {
     var alerts = await _appDbContext.MaintenanceAlerts
                .Include(ma => ma.Car)
                .ThenInclude(c => c!.Brand)
                .ToListAsync();
            if(alerts.Count() == 0)
        {
            return null;
        }
            return _mapper.Map<List<MaintenanceAlertDto>>(alerts);
    }

    public async Task<MaintenanceAlertDto> UpdateMaintenanceAlertStatusAsync(int alertId, string status)
    {
       var alert = await _appDbContext.MaintenanceAlerts
                .Include(a => a.Car)
                .ThenInclude(c => c!.Brand)
                .FirstOrDefaultAsync(a => a.MaintenanceAlertId == alertId);
            if (alert == null) throw new KeyNotFoundException("Maintenance alert ticket not found.");
            alert.Status = status;
            if (status.Equals("Fixed", StringComparison.OrdinalIgnoreCase))
            {
                if (alert.Car != null)
                {
                    alert.Car.CarStatusId = 1;
                }
            }

            await _appDbContext.SaveChangesAsync();
            return _mapper.Map<MaintenanceAlertDto>(alert);
    }
}