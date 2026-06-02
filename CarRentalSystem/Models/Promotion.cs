using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace CarRentalSystem.Models;
public class Promotion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PromotionId { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Range(0, 100)]
        public int DiscountPercent { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        public bool Active { get; set; } = true;
    }