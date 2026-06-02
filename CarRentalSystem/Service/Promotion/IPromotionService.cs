using CarRentalSystem.DTO.Promotion;

namespace CarRentalSystem.Service.Promotions;
public interface IPromotionService
    {
       Task<List<PromotionDto>> GetAllPromotionsAsync();
        Task<PromotionDto?> GetPromotionByIdAsync(int id);
        Task<PromotionDto> AddPromotionAsync(CreatePromotionRequest req);
        Task<bool> DeletePromotionAsync(int id);
        Task<bool> TogglePromoStatusAsync(int id);
        Task<PromotionDto?> ValidatePromoCodeAsync(string code);
    }