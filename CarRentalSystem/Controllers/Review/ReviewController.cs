using CarRentalSystem.DTO.Common;
using CarRentalSystem.DTO.Review;
using CarRentalSystem.Service.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers.v1;

[Route("api/v1/reviews")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService) => _reviewService = reviewService;

    /// <summary>Public — all reviews paginated.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetAllReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetAllReviewsAsync(page, pageSize);
        return Ok(result);
    }

    /// <summary>Public — reviews for a specific car paginated.</summary>
    [HttpGet("car/{carId}")]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetCarReviews(
        int carId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetCarReviewsAsync(carId, page, pageSize);
        return Ok(result);
    }

    /// <summary>Customer only — submit a review for a completed rental.</summary>
    [HttpPost]
    [Authorize(Roles = "Customer")]
    public async Task<ActionResult<ReviewDto>> AddReview(
        [FromQuery] int userId,
        [FromBody] CreateReviewRequest request)
    {
        var review = await _reviewService.AddReviewAsync(userId, request);
        if (review.Result is NotFoundObjectResult notFound) return notFound;
        if (review.Result is BadRequestObjectResult bad) return bad;
        return CreatedAtAction(nameof(GetAllReviews), review.Value);
    }
}