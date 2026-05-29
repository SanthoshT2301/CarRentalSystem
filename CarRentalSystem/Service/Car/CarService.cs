using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Cars;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Car;

public class CarService : ICarService
{

private readonly AppDbContext _appDbContext;
public CarService(AppDbContext appDbContext)
{
    _appDbContext = appDbContext;
}    
    public async Task<ActionResult<IEnumerable<CarDto>>> GetAllCars()
    {
        var cars = await _appDbContext.Cars
            .Include(c => c.Brand)
            .Include(c => c.Category)
            .Include(c => c.Location)
            .Include(c => c.CarImages)
            .Select(c => new CarDto
            {
                Id = c.CarId,
                Make = c.Brand.BrandName,
                Model = c.Model,
                Year = (int)c.CarYear,
                Type = c.Category.CategoryName,
                Location = c.Location.LocationName,
                PricePerDay = (decimal)c.PricePerDay,
                Available = c.CarStatusId == 1,
                Image = c.CarImages.FirstOrDefault() != null ? c.CarImages.FirstOrDefault().ImageUrl : ""
            }).ToListAsync();

        return cars;
    }

    public async Task<ActionResult<CarDto>> GetCarById(int id)
{
    var car = await _appDbContext.Cars
        .Include(c => c.Brand)
        .Include(c => c.Category)
        .Include(c => c.Location)
        .Include(c => c.CarImages)
        .FirstOrDefaultAsync(c => c.CarId == id);

    if (car == null)
    {
        return null;
    }

    var carDto = new CarDto
    {
        Id = car.CarId,
        Make = car.Brand?.BrandName,
        Model = car.Model,
        Year = (int)car.CarYear,
        Type = car.Category?.CategoryName,
        Location = car.Location?.LocationName,
        PricePerDay = (decimal)car.PricePerDay,
        Available = car.CarStatusId == 1,
        Image = car.CarImages.FirstOrDefault()?.ImageUrl ?? ""
    };

    return carDto;
}
}