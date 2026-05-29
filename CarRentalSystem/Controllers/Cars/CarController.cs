using CarRentalSystem.DTO.Cars;
using CarRentalSystem.Models;
using CarRentalSystem.Service.Car;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Cars;
public class CarController : ControllerBase
{
    private readonly ICarService _carService;
    public CarController(ICarService carService)
    {
        _carService = carService;
    }
    [HttpGet("GetAllCars")]
    public async Task<ActionResult<IEnumerable<CarDto>>> GetAllCars()
    {
        var cars= await _carService.GetAllCars();
        if (cars!=null)
        {
            return Ok(cars);
        }
        return NotFound();
    }
    [HttpGet("GetCarById/{id}")]
    public async Task<ActionResult<CarDto>> GetCarById(int id)
    {
        var car= await _carService.GetCarById(id);
         if (car!=null)
        {
            return Ok(car);
        }
        return NotFound();
    }
}

    