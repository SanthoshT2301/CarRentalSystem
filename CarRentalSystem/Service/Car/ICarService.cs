using CarRentalSystem.DTO.Cars;
using CarRentalSystem.DTO.Common;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Car;

public interface ICarService
{
    Task<PagedResult<CarDto>> GetAllCars(int page, int pageSize);
    Task<ActionResult<CarDto>> GetCarById(int id);
    Task<ActionResult<CarDto>> CreateCarAsync(CreateCarRequest request);
    Task<ActionResult<bool>> DeleteCarAsync(int id);
}