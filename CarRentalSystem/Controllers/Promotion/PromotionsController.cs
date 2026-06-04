using CarRentalSystem.DTO.Promotion;
using CarRentalSystem.Service.Promotions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalSystem.Controllers.v1;

[ApiController]
[Route("api/v1/promotions")]
public class PromotionsController : ControllerBase
{
    private readonly IPromotionService _promotionService;
    public PromotionsController(IPromotionService promotionService) => _promotionService = promotionService;

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetPromotions()
    {
        var promos = await _promotionService.GetAllPromotionsAsync();
        return promos is null ? NotFound(new { message = "No promotions found." }) : Ok(promos);
    }

    [HttpGet("validate/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> ValidatePromo(string code)
    {
        var promo = await _promotionService.ValidatePromoCodeAsync(code);
        return promo is null ? NotFound(new { message = "Invalid or inactive promotion code." }) : Ok(promo);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddPromotion([FromBody] CreatePromotionRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var newPromo = await _promotionService.AddPromotionAsync(request);
        return CreatedAtAction(nameof(GetPromotions), new { id = newPromo.PromotionId }, newPromo);
    }

    [HttpPut("toggle/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> TogglePromo(int id)
    {
        var outcome = await _promotionService.TogglePromoStatusAsync(id);
        return Ok(new { promotionId = id, activeStatus = outcome });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePromo(int id)
    {
        var outcome = await _promotionService.DeletePromotionAsync(id);
        return outcome ? NoContent() : NotFound(new { message = "Promotion not found." });
    }
}