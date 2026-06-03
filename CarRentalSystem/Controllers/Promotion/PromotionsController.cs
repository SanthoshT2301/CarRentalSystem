using CarRentalSystem.DTO.Promotion;
using CarRentalSystem.Service.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;

    public PromotionsController(IPromotionService promotionService)
        => _promotionService = promotionService;

    // Public — customers can fetch active promo list
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPromotions()
    {
        var promos = await _promotionService.GetAllPromotionsAsync();
        return promos is null ? NotFound(new { message = "No promotions found." }) : Ok(promos);
    }

    // Public — validate a promo code at checkout
    [HttpGet("validate/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidatePromo(string code)
    {
        var promo = await _promotionService.ValidatePromoCodeAsync(code);
        return promo is null
            ? NotFound(new { message = "Invalid or inactive promotion code." })
            : Ok(promo);
    }

    // Admin only — create a new promotion
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddPromotion([FromBody] CreatePromotionRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var newPromo = await _promotionService.AddPromotionAsync(request);
        return CreatedAtAction(nameof(GetPromotions), new { id = newPromo.PromotionId }, newPromo);
    }

    // Admin only — toggle active/inactive state
    [HttpPut("toggle/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TogglePromo(int id)
    {
        var outcome = await _promotionService.TogglePromoStatusAsync(id);
        return Ok(new { promotionId = id, activeStatus = outcome });
    }

    // Admin only — remove a promotion
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePromo(int id)
    {
        var outcome = await _promotionService.DeletePromotionAsync(id);
        return outcome
            ? NoContent()
            : NotFound(new { message = "Promotion not found." });
    }
}