using CarRentalSystem.DTO.Cars;
using CarRentalSystem.DTO.Common;
using CarRentalSystem.Service.Car;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers.v1;

[Route("api/v1/cars")]
[ApiController]
public class CarController : ControllerBase
{
    private readonly ICarService _carService;

    public CarController(ICarService carService) => _carService = carService;

   
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<CarDto>>> GetAllCars(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _carService.GetAllCars(page, pageSize);
        return Ok(result);
    }

   
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CarDto>> GetCarById(int id)
    {
        var car = await _carService.GetCarById(id);
        return car.Value is not null ? Ok(car.Value) : NotFound(new { error = $"Car {id} not found." });
    }

    
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<CarDto>> CreateCar([FromBody] CreateCarRequest request)
    {
        var car = await _carService.CreateCarAsync(request);
        return car.Value is not null
            ? CreatedAtAction(nameof(GetCarById), new { id = car.Value.Id }, car.Value)
            : BadRequest(new { error = "Failed to create car." });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        var result = await _carService.DeleteCarAsync(id);
        if (result.Result is NotFoundObjectResult notFound) return notFound;
        return Ok(new { message = "Car deleted successfully." });
    }
}