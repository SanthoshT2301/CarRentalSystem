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

  
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetAllReviews(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetAllReviewsAsync(page, pageSize);
        return Ok(result);
    }


    [HttpGet("agent/{agentId}")]
    [Authorize(Roles = "Agent,Admin")]
    public async Task<ActionResult<PagedResult<ReviewDto>>> GetAgentCarReviews(
    int agentId,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        var result = await _reviewService.GetAgentCarReviewsAsync(agentId, page, pageSize);
        return Ok(result);
    }

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