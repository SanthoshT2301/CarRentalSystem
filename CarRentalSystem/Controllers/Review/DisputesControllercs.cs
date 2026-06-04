using CarRentalSystem.Service.Review;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers.Review
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] // Review dispute moderations are restricted strictly to Administrators
    public class DisputesController : ControllerBase
    {
        private readonly IReviewDisputeServicecs _disputeService;

        public DisputesController(IReviewDisputeServicecs disputeService)
        {
            _disputeService = disputeService;
        }

        [HttpPost("dispute/{reviewId}")]
        public async Task<IActionResult> FlagDispute(int reviewId, [FromBody] FlagDisputeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Resolution))
            {
                return BadRequest(new { message = "Dispute resolution/disagreement grounds explanation is required" });
            }

            try
            {
                var response = await _disputeService.DisputeReviewAsync(reviewId, request.Resolution);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("resolve/{reviewId}")]
        public async Task<IActionResult> ResolveDispute(int reviewId, [FromBody] ResolveDisputeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Action) ||
                (!request.Action.Equals("keep", StringComparison.OrdinalIgnoreCase) &&
                 !request.Action.Equals("remove", StringComparison.OrdinalIgnoreCase)))
            {
                return BadRequest(new { message = "Resolution action must be either 'keep' or 'remove'" });
            }

            try
            {
                var success = await _disputeService.ResolveDisputeAsync(reviewId, request.Action);
                if (!success) return NotFound(new { message = "Review not found" });

                return Ok(new { message = $"Review dispute resolved with action: {request.Action.ToLower()}" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    public class FlagDisputeRequest
    {
        public string Resolution { get; set; } = string.Empty;
    }

    public class ResolveDisputeRequest
    {
        public string Action { get; set; } = string.Empty; 
    }
}
