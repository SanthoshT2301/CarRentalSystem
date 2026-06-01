using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Review;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Review;

public class ReviewService : IReviewService
{
    private readonly AppDbContext _appDbContext;
    public ReviewService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<ActionResult<ReviewDto>> AddReviewAsync(int userID, CreateReviewRequest request)
    {
        var reservation = await _appDbContext.Reservations
            .Include(r => r.Car)
            .FirstOrDefaultAsync(r => r.ReservationId == request.ReservationId);
        var user = await _appDbContext.Users.FindAsync(userID);

        if (user == null)
        {
            return new NotFoundObjectResult("User not found");
        }
        if (reservation==null)
        {
            return new NotFoundObjectResult("Reservation not found");
        }
        if(reservation.UserId!=userID)
        {
            return new BadRequestObjectResult("You cannot review your own reservation");
        }
        if(reservation.ReservationStatusId!=2)
        {
            return new BadRequestObjectResult("You can only review a car after completed");
        }
        var existingReview=await _appDbContext.Reviews.FirstOrDefaultAsync(r=>r.ReservationId==request.ReservationId);
        if(existingReview!=null)
        {
            return new BadRequestObjectResult("You have already reviewed this reservation");
        }
        var review=new CarRentalSystem.Models.Review
        {
            UserId=userID,
            ReservationId=request.ReservationId,
            Rating=request.Rating,
            Comment=request.Comment,
            CreatedAt=DateTime.UtcNow
        };
        _appDbContext.Reviews.Add(review);
        await _appDbContext.SaveChangesAsync();
        
        return new ReviewDto
        {
            ReviewId=review.ReviewId,
            UserId=userID,
            ReservationId=review.ReservationId,
            CarId=reservation.CarId,
            CarName=reservation.Car.Model,
            UserName=user.FirstName+" "+user.LastName,
            Rating=review.Rating,
            Comment=review.Comment,
            CreatedAt=review.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty
        };
    }

    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAllReviewsAsync()
    {
        var reviews=await _appDbContext.Reviews
        .Include(r=>r.User)
        .Include(r=>r.Reservation)
        .Include(r=>r.Reservation.Car)
        .OrderByDescending(r=>r.CreatedAt)
        .ToListAsync();
        var reviewsdto=new List<ReviewDto>();
        foreach(var review in reviews)
        {
            reviewsdto.Add(new ReviewDto
            {
            ReviewId=review.ReviewId,
            UserId=review.UserId,
            ReservationId=review.ReservationId,
            CarId=review.Reservation.CarId,
            CarName=review.Reservation.Car.Model,
            Rating=review.Rating,
            Comment=review.Comment,
            CreatedAt=review.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty
            });
        }
        return reviewsdto;
    }

    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetCarReviewsAsync(int carId)
{
    var car = await _appDbContext.Cars
        .Include(c => c.Brand)
        .FirstOrDefaultAsync(c => c.CarId == carId);

    if (car == null)
    {
        return new NotFoundObjectResult("Car not found");
    }

    var reviews = await _appDbContext.Reviews
        .Include(r => r.User)
        .Include(r => r.Reservation)
        .ThenInclude(res => res.Car)
        .Where(r => r.Reservation != null && r.Reservation.CarId == carId)
        .OrderByDescending(r => r.CreatedAt)
        .ToListAsync();

    var reviewsdto = reviews.Select(review => new ReviewDto
    {
        ReviewId = review.ReviewId,
        UserId = review.UserId,
        ReservationId = review.ReservationId,
        CarId = carId,
        CarName = review.Reservation?.Car?.Model ?? "",
        UserName = review.User != null
            ? $"{review.User.FirstName} {review.User.LastName}"
            : "Unknown User",
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty
    }).ToList();

    return reviewsdto;
}

    public async Task<ActionResult<int>> GetReviewCountForCarAsync(int carId)
    {
        return await _appDbContext.Reviews
                .CountAsync(r => r.Reservation != null && r.Reservation.CarId == carId);
    }
    public async Task<ActionResult<double>> GetAverageRatingForCarAsync(int carId)
    {
        var ratings = await _appDbContext.Reviews
                .Where(r => r.Reservation != null && r.Reservation.CarId == carId && r.Rating != null)
                .Select(r => r.Rating.Value)
                .ToListAsync();

            if (!ratings.Any())
            {
                return 4.8;
            }

            return Math.Round(ratings.Average(), 1);
    }
}