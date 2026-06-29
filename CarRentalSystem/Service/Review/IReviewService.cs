using CarRentalSystem.DTO.Common;
using CarRentalSystem.DTO.Review;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Service.Review;

public interface IReviewService
{
    Task<ActionResult<ReviewDto>> AddReviewAsync(int userID, CreateReviewRequest request);
    Task<PagedResult<ReviewDto>> GetCarReviewsAsync(int carId, int page, int pageSize);
    Task<PagedResult<ReviewDto>> GetAllReviewsAsync(int page, int pageSize);
    Task<ActionResult<double>> GetAverageRatingForCarAsync(int carId);
    Task<ActionResult<int>> GetReviewCountForCarAsync(int carId);
    Task<PagedResult<ReviewDto>> GetAgentCarReviewsAsync(int agentId, int page, int pageSize);
}