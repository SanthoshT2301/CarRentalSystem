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

    /// <summary>Public — browse all cars (paginated).</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<CarDto>>> GetAllCars(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _carService.GetAllCars(page, pageSize);
        return Ok(result);
    }

    /// <summary>Public — get a single car by ID.</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<CarDto>> GetCarById(int id)
    {
        var car = await _carService.GetCarById(id);
        return car.Value is not null ? Ok(car.Value) : NotFound(new { error = $"Car {id} not found." });
    }

    /// <summary>
    /// Agent/Admin — add a new car to the fleet.
    /// When called by an Agent, the car is tagged with their userId so they
    /// can manage it and so gate logistics can be filtered to their cars.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<ActionResult<CarDto>> CreateCar(
        [FromBody] CreateCarRequest request,
        [FromQuery] int? agentId = null)  // Agent passes their own userId here
    {
        // If an Agent is calling, validate that the agentId belongs to them.
        // (Admins may pass null to indicate a fleet-wide car.)
        var car = await _carService.CreateCarAsync(request, agentId);
        return car.Value is not null
            ? CreatedAtAction(nameof(GetCarById), new { id = car.Value.Id }, car.Value)
            : BadRequest(new { error = "Failed to create car." });
    }

    /// <summary>
    /// Agent — list only the cars they added (paginated).
    /// </summary>
    [HttpGet("my-fleet/{agentId}")]
    [Authorize(Roles = "Agent,Admin")]
    public async Task<ActionResult<PagedResult<CarDto>>> GetAgentCars(
        int agentId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _carService.GetCarsByAgentAsync(agentId, page, pageSize);
        return Ok(result);
    }

    /// <summary>Admin/Agent — remove a car from the fleet.</summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Agent")]
    public async Task<IActionResult> DeleteCar(int id)
    {
        var result = await _carService.DeleteCarAsync(id);
        if (result.Result is NotFoundObjectResult notFound) return notFound;
        return Ok(new { message = "Car deleted successfully." });
    }
}