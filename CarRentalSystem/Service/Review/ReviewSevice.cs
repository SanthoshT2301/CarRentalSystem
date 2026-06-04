using AutoMapper;
using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Common;
using CarRentalSystem.DTO.Review;
using CarRentalSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Review;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ReviewService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ActionResult<ReviewDto>> AddReviewAsync(int userId, CreateReviewRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return new NotFoundObjectResult("User not found.");

        var reservation = await _context.Reservations
            .Include(r => r.Car).ThenInclude(c => c!.Brand)
            .FirstOrDefaultAsync(r => r.ReservationId == request.ReservationId);
        if (reservation == null) return new NotFoundObjectResult("Reservation not found.");
        if (reservation.UserId != userId) return new BadRequestObjectResult("You can only review your own reservations.");
        if (reservation.ReservationStatusId != 2) return new BadRequestObjectResult("You can only review a car after the rental is completed.");

        var existing = await _context.Reviews.AnyAsync(r => r.ReservationId == request.ReservationId);
        if (existing) return new BadRequestObjectResult("You have already reviewed this reservation.");

        var review = _mapper.Map<Models.Review>(request);
        review.UserId = userId;
        review.CreatedAt = DateTime.UtcNow;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        var loaded = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Reservation).ThenInclude(res => res!.Car).ThenInclude(c => c!.Brand)
            .FirstAsync(r => r.ReviewId == review.ReviewId);
        return _mapper.Map<ReviewDto>(loaded);
    }

    // paginated
    public async Task<PagedResult<ReviewDto>> GetCarReviewsAsync(int carId, int page, int pageSize)
    {
        var carExists = await _context.Cars.AnyAsync(c => c.CarId == carId);
        if (!carExists) throw new KeyNotFoundException("Car not found.");

        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Reservation).ThenInclude(res => res!.Car).ThenInclude(c => c!.Brand)
            .Where(r => r.Reservation != null && r.Reservation.CarId == carId)
            .OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReviewDto>
        {
            Data = _mapper.Map<List<ReviewDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    // paginated
    public async Task<PagedResult<ReviewDto>> GetAllReviewsAsync(int page, int pageSize)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Reservation).ThenInclude(res => res!.Car).ThenInclude(c => c!.Brand)
            .OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ReviewDto>
        {
            Data = _mapper.Map<List<ReviewDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<ActionResult<int>> GetReviewCountForCarAsync(int carId) =>
        await _context.Reviews.CountAsync(r => r.Reservation != null && r.Reservation.CarId == carId);

    public async Task<ActionResult<double>> GetAverageRatingForCarAsync(int carId)
    {
        var ratings = await _context.Reviews
            .Where(r => r.Reservation != null && r.Reservation.CarId == carId && r.Rating != null)
            .Select(r => r.Rating!.Value)
            .ToListAsync();
        return ratings.Any() ? Math.Round(ratings.Average(), 1) : 4.8;
    }
}