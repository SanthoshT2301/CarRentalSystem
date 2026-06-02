namespace CarRentalSystem.DTO.Promotion;
public class PromotionDto
    {
        public int PromotionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public int DiscountPercent { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
    }