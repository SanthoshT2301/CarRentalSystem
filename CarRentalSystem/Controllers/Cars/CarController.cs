using CarRentalSystem.DTO.Cars;
using CarRentalSystem.Service.Car;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Cars;

[Route("api/[controller]")]
[ApiController]
public class CarController : ControllerBase
{
    private readonly ICarService _carService;

    public CarController(ICarService carService) => _carService = carService;

    // Public — anyone can browse available cars
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CarDto>>> GetAllCars()
    {
        var cars = await _carService.GetAllCars();
        return cars.Value is not null ? Ok(cars.Value) : NotFound(new { error = "No cars found." });
    }

    // Public — anyone can view a single car
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CarDto>> GetCarById(int id)
    {
        var car = await _carService.GetCarById(id);
        return car.Value is not null ? Ok(car.Value) : NotFound(new { error = $"Car {id} not found." });
    }

    // Admin only — add a new car to the fleet
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CarDto>> CreateCarAsync([FromBody] CreateCarRequest request)
    {
        var car = await _carService.CreateCarAsync(request);
        return car.Value is not null
            ? CreatedAtAction(nameof(GetCarById), new { id = car.Value.Id }, car.Value)
            : BadRequest(new { error = "Failed to create car." });
    }

    // Admin only — remove a car from the fleet
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<bool>> DeleteCarAsync(int id)
    {
        var result = await _carService.DeleteCarAsync(id);
        if (result.Result is NotFoundObjectResult notFound) return notFound;
        return Ok(new { message = "Car deleted successfully." });
    }
}