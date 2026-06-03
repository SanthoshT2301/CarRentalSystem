using AutoMapper;
using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Cars;
using CarRentalSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Car;

public class CarService : ICarService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CarService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // ── GET ALL ──────────────────────────────────────────────────────────────
    public async Task<ActionResult<IEnumerable<CarDto>>> GetAllCars()
    {
        var cars = await _context.Cars
            .Include(c => c.Brand)
            .Include(c => c.Category)
            .Include(c => c.FuelType)
            .Include(c => c.CarStatus)
            .Include(c => c.Location)
            .Include(c => c.CarImages)
            .ToListAsync();

        return _mapper.Map<List<CarDto>>(cars);
    }

    // ── GET BY ID ─────────────────────────────────────────────────────────────
    public async Task<ActionResult<CarDto>> GetCarById(int id)
    {
        var car = await _context.Cars
            .Include(c => c.Brand)
            .Include(c => c.Category)
            .Include(c => c.FuelType)
            .Include(c => c.CarStatus)
            .Include(c => c.Location)
            .Include(c => c.CarImages)
            .FirstOrDefaultAsync(c => c.CarId == id);

        if (car == null) return new NotFoundObjectResult($"Car with id {id} not found.");

        return _mapper.Map<CarDto>(car);
    }

    // ── CREATE ────────────────────────────────────────────────────────────────
    public async Task<ActionResult<CarDto>> CreateCarAsync(CreateCarRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Make) || string.IsNullOrWhiteSpace(request.Model))
            throw new ArgumentException("Make and Model are required.");

        // Resolve or create Brand
        var brand = await _context.CarBrands
            .FirstOrDefaultAsync(b => b.BrandName.ToLower() == request.Make.ToLower())
            ?? await CreateAndSaveAsync(_context.CarBrands, new CarBrand
            {
                BrandName = request.Make,
                LogoUrl = request.Image,
                IsActive = true
            });

        // Resolve or create Category
        var category = await _context.CarCategories
            .FirstOrDefaultAsync(c => c.CategoryName.ToLower() == request.Type.ToLower())
            ?? await CreateAndSaveAsync(_context.CarCategories, new CarCategory
            {
                CategoryName = request.Type
            });

        // Resolve or create Location
        var location = await _context.Locations
            .FirstOrDefaultAsync(l => l.LocationName.ToLower() == request.Location.ToLower())
            ?? await CreateAndSaveAsync(_context.Locations, new Location
            {
                LocationName = request.Location
            });

        // Resolve FuelType from features list
        var fuelTypeName = ResolveFuelType(request.Features);
        var fuelType = await _context.FuelTypes
            .FirstOrDefaultAsync(f => f.FuelTypeName.ToLower() == fuelTypeName.ToLower())
            ?? await CreateAndSaveAsync(_context.FuelTypes, new FuelType { FuelTypeName = fuelTypeName });

        // Resolve or create CarStatus "Available"
        var status = await _context.CarStatuses.FirstOrDefaultAsync(s => s.StatusName == "Available")
            ?? await CreateAndSaveAsync(_context.CarStatuses, new CarStatus { StatusName = "Available" });

        var car = new Models.Car
        {
            BrandId = brand.BrandId,
            CategoryId = category.CategoryId,
            LocationId = location.LocationId,
            FuelTypeId = fuelType.FuelTypeId,
            CarStatusId = status.CarStatusId,
            Model = request.Model.Trim(),
            CarYear = request.Year > 0 ? request.Year : 2023,
            PricePerDay = request.PricePerDay > 0 ? request.PricePerDay : 50.00m,
            Transmission = request.Transmission.Trim(),
            Mileage = request.Mileage,
            Color = request.Color.Trim(),
            NoSeats = request.NoSeats > 0 ? request.NoSeats : 5,
            Address = string.IsNullOrWhiteSpace(request.Address)
                            ? $"123 Rental Blvd, {location.LocationName}"
                            : request.Address.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.Cars.Add(car);
        await _context.SaveChangesAsync();

        _context.CarImages.Add(new CarImage { CarId = car.CarId, ImageUrl = request.Image });
        await _context.SaveChangesAsync();

        // Reload with all navigations for accurate mapping
        var loaded = await _context.Cars
            .Include(c => c.Brand)
            .Include(c => c.Category)
            .Include(c => c.FuelType)
            .Include(c => c.CarStatus)
            .Include(c => c.Location)
            .Include(c => c.CarImages)
            .FirstAsync(c => c.CarId == car.CarId);

        return _mapper.Map<CarDto>(loaded);
    }

    // ── DELETE ────────────────────────────────────────────────────────────────
    public async Task<ActionResult<bool>> DeleteCarAsync(int id)
    {
        var car = await _context.Cars.Include(c => c.CarImages).FirstOrDefaultAsync(c => c.CarId == id);
        if (car == null) return new NotFoundObjectResult($"Car with id {id} not found.");

        _context.CarImages.RemoveRange(car.CarImages);
        _context.Cars.Remove(car);
        await _context.SaveChangesAsync();
        return true;
    }

    // ── HELPERS ───────────────────────────────────────────────────────────────
    private async Task<T> CreateAndSaveAsync<T>(Microsoft.EntityFrameworkCore.DbSet<T> dbSet, T entity)
        where T : class
    {
        dbSet.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    private static string ResolveFuelType(List<string> features)
    {
        if (features.Any(f => f.ToLower().Contains("electric"))) return "Electric";
        if (features.Any(f => f.ToLower().Contains("hybrid"))) return "Hybrid";
        if (features.Any(f => f.ToLower().Contains("diesel"))) return "Diesel";
        if (features.Any(f => f.ToLower().Contains("petrol"))) return "Petrol";
        return "Gasoline";
    }
}