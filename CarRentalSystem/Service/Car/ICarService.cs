using CarRentalSystem.DTO.Cars;
using CarRentalSystem.DTO.Common;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Car;

public interface ICarService
{
    Task<PagedResult<CarDto>> GetAllCars(int page, int pageSize);
    Task<ActionResult<CarDto>> GetCarById(int id);

    /// <summary>Create a car. Pass agentId when called from an Agent context, null for Admin.</summary>
    Task<ActionResult<CarDto>> CreateCarAsync(CreateCarRequest request, int? agentId = null);

    Task<ActionResult<bool>> DeleteCarAsync(int id);

    /// <summary>Returns only cars added by the given agent.</summary>
    Task<PagedResult<CarDto>> GetCarsByAgentAsync(int agentId, int page, int pageSize);
}