using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class FuelType
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FuelTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string FuelTypeName { get; set; } = string.Empty;
    }
}
