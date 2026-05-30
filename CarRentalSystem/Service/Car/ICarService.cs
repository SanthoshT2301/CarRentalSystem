using CarRentalSystem.DTO.Cars;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Car;
public interface ICarService
{
    Task<ActionResult<IEnumerable<CarDto>>> GetAllCars();
    Task<ActionResult<CarDto>> GetCarById(int id);
    Task<ActionResult<CarDto>> CreateCarAsync(CreateCarRequest request);
    Task<ActionResult<bool>> DeleteCarAsync(int id);
}