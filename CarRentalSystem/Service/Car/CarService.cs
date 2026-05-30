using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Cars;
using CarRentalSystem.Models;
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

    public async Task<ActionResult<CarDto>> CreateCarAsync(CreateCarRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.Make) || string.IsNullOrWhiteSpace(request.Model)){
            throw new ArgumentException("Make and Model are required.");
        }
        var brand=await _appDbContext.CarBrands.Where(b=>b.BrandName.ToLower()==request.Make.ToLower()).FirstOrDefaultAsync();
        if (brand == null)
        {
            var b=new CarBrand
            {
                BrandName=request.Make,
                LogoUrl=request.Image,
                IsActive=true
            };
            _appDbContext.CarBrands.AddAsync(b);
            await _appDbContext.SaveChangesAsync();
        }
        var category=await _appDbContext.CarCategories.Where(c=>c.CategoryName.ToLower()==request.Type.ToLower()).FirstOrDefaultAsync();
        if (category == null)
        {
            var c=new CarCategory
            {
            CategoryName=request.Type,
            Description=null
            };
            _appDbContext.CarCategories.AddAsync(c);
            await _appDbContext.SaveChangesAsync();
        }
        var location=await _appDbContext.Locations.Where(c=>c.LocationName.ToLower()==request.Location.ToLower()).FirstOrDefaultAsync();
        if (location == null)
        {
            var l=new Location
            {
            LocationName=request.Location
            };
            _appDbContext.Locations.AddAsync(l);
            await _appDbContext.SaveChangesAsync();
        }
        var FuelType="Gasoline";
        if(request.Features.Any(f=>f.ToLower().Contains("diesel"))){
            FuelType="Diesel";
        }else if (request.Features.Any(f=>f.ToLower().Contains("Electric")))
        {
            FuelType="Electric";
        }else if (request.Features.Any(f=>f.ToLower().Contains("Hybrid")))
        {
            FuelType="Hybrid";
        } else if (request.Features.Any(f=>f.ToLower().Contains("Petrol")))
        {
            FuelType="Petrol";
        }
        var fuel=await _appDbContext.FuelTypes.Where(c=>c.FuelTypeName.ToLower()==FuelType.ToLower()).FirstOrDefaultAsync();
        if (fuel == null)
        {
            var f=new FuelType
            {
            FuelTypeName=FuelType
            };
            _appDbContext.FuelTypes.AddAsync(f);
            await _appDbContext.SaveChangesAsync();
        }
        var status = await _appDbContext.CarStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available");
            if (status == null)
            {
                status = new CarStatus { StatusName = "Available" };
                _appDbContext.CarStatuses.Add(status);
                await _appDbContext.SaveChangesAsync();
            }
        var newCar = new Models.Car
            {
                BrandId = brand.BrandId,
                CategoryId = category.CategoryId,
                LocationId = location.LocationId,
                FuelTypeId = fuel.FuelTypeId,
                CarStatusId = status.CarStatusId,
                Model = request.Model.Trim(),
                CarYear = request.Year > 0 ? request.Year : 2023,
                PricePerDay = request.PricePerDay > 0 ? request.PricePerDay : 50.00m,
                Transmission = request.Transmission.Trim(),
                Mileage = request.Mileage,
                Color = request.Color.Trim(),
                NoSeats = 5,
                Address = string.IsNullOrWhiteSpace(request.Address) ? "123 Rental Blvd, " + (location?.LocationName ?? "San Francisco") : request.Address.Trim(),
                CreatedAt = DateTime.UtcNow
            };
            _appDbContext.Cars.Add(newCar);
            await _appDbContext.SaveChangesAsync();
            var image=request.Image;
            var carImage=new CarImage
            {
                CarId=newCar.CarId,
                ImageUrl=image
            };
            _appDbContext.CarImages.Add(carImage);
            await _appDbContext.SaveChangesAsync();
            var loadedCar = await _appDbContext.Cars
                .Include(c => c.Brand)
                .Include(c => c.Category)
                .Include(c => c.FuelType)
                .Include(c => c.CarStatus)
                .Include(c => c.Location)
                .Include(c => c.CarImages)
                .FirstAsync(c => c.CarId == newCar.CarId);
            var carDto=new CarDto
            {
                Id=loadedCar.CarId,
                Make=loadedCar.Brand.BrandName,
                Model=loadedCar.Model,
                Year=(int)loadedCar.CarYear,
                Type=loadedCar.Category.CategoryName,
                Location=loadedCar.Location.LocationName,
                PricePerDay=(decimal)loadedCar.PricePerDay,
                Available=loadedCar.CarStatusId==1,
                Image=loadedCar.CarImages.FirstOrDefault()?.ImageUrl??""
            };
        return carDto;
    }

    public async Task<ActionResult<bool>> DeleteCarAsync(int id)
    {
        var car=await _appDbContext.Cars.FindAsync(id);
        if(car==null)
        {
            return false;
        }
        var cRemove=await _appDbContext.CarImages.Where(c=>c.CarId==id).FirstOrDefaultAsync();
        _appDbContext.CarImages.Remove(cRemove);
        await _appDbContext.SaveChangesAsync();
        _appDbContext.Cars.Remove(car);
        await _appDbContext.SaveChangesAsync();
        return true;
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