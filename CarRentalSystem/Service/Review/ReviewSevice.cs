using AutoMapper;
using CarRentalSystem.DATA;
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

    // ── ADD REVIEW ────────────────────────────────────────────────────────────
    public async Task<ActionResult<ReviewDto>> AddReviewAsync(int userId, CreateReviewRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return new NotFoundObjectResult("User not found.");

        var reservation = await _context.Reservations
            .Include(r => r.Car)
            .ThenInclude(c => c!.Brand)
            .FirstOrDefaultAsync(r => r.ReservationId == request.ReservationId);

        if (reservation == null) return new NotFoundObjectResult("Reservation not found.");

        if (reservation.UserId != userId)
            return new BadRequestObjectResult("You can only review your own reservations.");

        if (reservation.ReservationStatusId != 2)
            return new BadRequestObjectResult("You can only review a car after the rental is completed.");

        var existing = await _context.Reviews.AnyAsync(r => r.ReservationId == request.ReservationId);
        if (existing)
            return new BadRequestObjectResult("You have already reviewed this reservation.");

        var review = _mapper.Map<Models.Review>(request);
        review.UserId = userId;
        review.CreatedAt = DateTime.UtcNow;

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        // Reload with navigations for accurate mapping
        var loaded = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Reservation)
            .ThenInclude(res => res!.Car)
            .ThenInclude(c => c!.Brand)
            .FirstAsync(r => r.ReviewId == review.ReviewId);

        return _mapper.Map<ReviewDto>(loaded);
    }

    // ── GET REVIEWS FOR CAR ───────────────────────────────────────────────────
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetCarReviewsAsync(int carId)
    {
        var carExists = await _context.Cars.AnyAsync(c => c.CarId == carId);
        if (!carExists) return new NotFoundObjectResult("Car not found.");

        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Reservation)
            .ThenInclude(res => res!.Car)
            .ThenInclude(c => c!.Brand)
            .Where(r => r.Reservation != null && r.Reservation.CarId == carId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ReviewDto>>(reviews);
    }

    // ── GET ALL REVIEWS ───────────────────────────────────────────────────────
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAllReviewsAsync()
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Reservation)
            .ThenInclude(res => res!.Car)
            .ThenInclude(c => c!.Brand)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ReviewDto>>(reviews);
    }

    // ── REVIEW COUNT ──────────────────────────────────────────────────────────
    public async Task<ActionResult<int>> GetReviewCountForCarAsync(int carId) =>
        await _context.Reviews.CountAsync(r => r.Reservation != null && r.Reservation.CarId == carId);

    // ── AVERAGE RATING ────────────────────────────────────────────────────────
    public async Task<ActionResult<double>> GetAverageRatingForCarAsync(int carId)
    {
        var ratings = await _context.Reviews
            .Where(r => r.Reservation != null && r.Reservation.CarId == carId && r.Rating != null)
            .Select(r => r.Rating!.Value)
            .ToListAsync();

        return ratings.Any() ? Math.Round(ratings.Average(), 1) : 4.8;
    }
}