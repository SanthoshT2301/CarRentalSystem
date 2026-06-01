using CarRentalSystem.DTO.Review;
using CarRentalSystem.Service.Review;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controller.Review;
[Route("api/[controller]/[action]")]
[ApiController]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;
    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }
    [HttpPost]
    public async Task<ActionResult<ReviewDto>> AddReviewAsync(int userID, CreateReviewRequest request)
    {
        var review= await _reviewService.AddReviewAsync(userID,request);
        if(review.Result is NotFoundObjectResult)
        {
            return review.Result;
        }
        if(review.Result is BadRequestObjectResult)
        {
            return review.Result;
        }
        return Ok(review.Value);
    }
    [HttpGet("car{carId}")]
     public async Task<ActionResult<IEnumerable<ReviewDto>>> GetCarReviews(int carId)
    {
        var reviews= await _reviewService.GetCarReviewsAsync(carId);
        if (!reviews.Value.Any())
        {
            return NotFound("No review for this car");
        }
        if(reviews.Result is NotFoundObjectResult)
        {
            return reviews.Result;
        }
        return Ok(reviews.Value);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetAllReviews()
    {
        var review=await _reviewService.GetAllReviewsAsync();
        if (!review.Value.Any())
        {
            return NotFound("No review found");
        }
        return Ok(review.Value);
    
    }
}