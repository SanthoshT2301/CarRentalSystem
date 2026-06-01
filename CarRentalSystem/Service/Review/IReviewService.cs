using CarRentalSystem.DTO.Review;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Review;
public interface IReviewService
{
    Task<ActionResult<ReviewDto>> AddReviewAsync(int userID, CreateReviewRequest request);
    Task<ActionResult<IEnumerable<ReviewDto>>> GetCarReviewsAsync(int carId);
    Task<ActionResult<IEnumerable<ReviewDto>>> GetAllReviewsAsync();
}