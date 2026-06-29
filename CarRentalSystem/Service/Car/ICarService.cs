using CarRentalSystem.DTO.Cars;
using CarRentalSystem.DTO.Common;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Car;

public interface ICarService
{
   
    Task<ActionResult<CarDto>> GetCarById(int id);

    /// <summary>Create a car. Pass agentId when called from an Agent context, null for Admin.</summary>
    Task<ActionResult<CarDto>> CreateCarAsync(CreateCarRequest request, int? agentId = null);

    Task<ActionResult<bool>> DeleteCarAsync(int id);

    /// <summary>Returns only cars added by the given agent.</summary>
    Task<PagedResult<CarDto>> GetCarsByAgentAsync(int agentId, int page, int pageSize);
    Task<ActionResult<CarDto>> UpdateCarAsync(int id, UpdateCarRequest request);
    Task<bool> CheckAvailabilityAsync(int carId, DateTime pickupDate, DateTime dropoffDate);
    Task<PagedResult<CarDto>> GetAllCars(int page, int pageSize, string? location = null, string? type = null, DateTime? pickupDate = null, DateTime? dropoffDate = null);
}