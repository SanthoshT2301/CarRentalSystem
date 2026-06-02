using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CarRentalSystem.DTO.Promotion;
using CarRentalSystem.Service.Promotions;

    [ApiController]
    [Route("api/[controller]")]
    public class PromotionsController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPromotions()
        {
            var promos = await _promotionService.GetAllPromotionsAsync();
            return Ok(promos);
        }

        [HttpGet("validate/{code}")]
        public async Task<IActionResult> ValidatePromo(string code)
        {
            var promo = await _promotionService.ValidatePromoCodeAsync(code);
            if (promo == null)
            {
                return NotFound(new { message = "Invalid or inactive promotion code" });
            }
            return Ok(promo);
        }

        [HttpPost]
       
        public async Task<IActionResult> AddPromotion([FromBody] CreatePromotionRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var newPromo = await _promotionService.AddPromotionAsync(request);
                return CreatedAtAction(nameof(GetPromotions), new { id = newPromo.PromotionId }, newPromo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("toggle/{id}")]
      
        public async Task<IActionResult> TogglePromo(int id)
        {
            try
            {
                var outcome = await _promotionService.TogglePromoStatusAsync(id);
                return Ok(new { promotionId = id, activeStatus = outcome });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
       
        public async Task<IActionResult> DeletePromo(int id)
        {
            var outcome = await _promotionService.DeletePromotionAsync(id);
            if (!outcome) return NotFound(new { message = "Promotion not found" });
            return NoContent();
        }
    }