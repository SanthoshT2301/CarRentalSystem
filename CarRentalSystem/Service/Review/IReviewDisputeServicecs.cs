using CarRentalSystem.DTO.Review;

namespace CarRentalSystem.Service.Review
{
    public interface IReviewDisputeServicecs
    {
        Task<ReviewDto> DisputeReviewAsync(int reviewId, string resolution);
        Task<bool> ResolveDisputeAsync(int reviewId, string action); // action: "keep" | "remove"
    }
}
