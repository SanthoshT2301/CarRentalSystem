using AutoMapper;
using CarRentalSystem.DATA;
using CarRentalSystem.DTO.Review;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Service.Review
{
    public class ReviewDisputeService : IReviewDisputeServicecs
    {
        private readonly AppDbContext appDbContext;
        private readonly IMapper mapper;
        public ReviewDisputeService(AppDbContext appDbContext,IMapper mapper)
        {
            this.appDbContext = appDbContext;
            this.mapper = mapper;
        }
        public async Task<ReviewDto> DisputeReviewAsync(int reviewId, string resolution)
        {
            var r = await appDbContext.Reviews
               .Include(rv => rv.User)
               .Include(rv => rv.Reservation)
               .ThenInclude(res => res!.Car)
               .ThenInclude(car => car!.Brand)
               .FirstOrDefaultAsync(rv => rv.ReviewId == reviewId);

            if (r == null) throw new ArgumentException("Review not found for dispute.");

            r.IsDisputed = true;
            r.DisputeResolution = resolution;

            await appDbContext.SaveChangesAsync();

            return mapper.Map<ReviewDto>(r);
        }

        public async Task<bool> ResolveDisputeAsync(int reviewId, string action)
        {
            var r = await appDbContext.Reviews.FindAsync(reviewId);
            if (r == null) return false;

            if (action.Equals("remove", StringComparison.OrdinalIgnoreCase))
            {
                appDbContext.Reviews.Remove(r);
            }
            else
            {
                r.IsDisputed = false;
                r.DisputeResolution = "Resolved: Feedback approved and retained.";
            }

            await appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
