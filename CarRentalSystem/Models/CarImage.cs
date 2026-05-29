using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CarRentalSystem.Models
{
    public class CarImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ImageId { get; set; }
        public int CarId { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        [JsonIgnore]
        [ForeignKey("CarId")]
        public Car? Car { get; set; }
    }
}
