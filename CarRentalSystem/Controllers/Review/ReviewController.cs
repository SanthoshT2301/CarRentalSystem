using CarRentalSystem.DTO.Review;
using CarRentalSystem.Service.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Review;

[Route("api/[controller]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService) => _reviewService = reviewService;

    // Public — anyone can read reviews
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAllReviews()
    {
        var reviews = await _reviewService.GetAllReviewsAsync();
        var list = reviews.Value?.ToList();
        return list is { Count: > 0 } ? Ok(list) : NotFound(new { error = "No reviews found." });
    }

    // Public — reviews for a specific car
    [HttpGet("car/{carId}")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetCarReviews(int carId)
    {
        var reviews = await _reviewService.GetCarReviewsAsync(carId);
        if (reviews.Result is NotFoundObjectResult notFound) return notFound;
        var list = reviews.Value?.ToList();
        return list is { Count: > 0 } ? Ok(list) : NotFound(new { error = "No reviews for this car." });
    }

    // Customer only — only a customer who completed a booking can review
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ReviewDto>> AddReview([FromQuery] int userId,
                                                          [FromBody] CreateReviewRequest request)
    {
        var review = await _reviewService.AddReviewAsync(userId, request);
        if (review.Result is NotFoundObjectResult notFound) return notFound;
        if (review.Result is BadRequestObjectResult bad) return bad;
        return CreatedAtAction(nameof(GetAllReviews), review.Value);
    }
}