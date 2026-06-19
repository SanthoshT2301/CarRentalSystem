using Microsoft.VisualBasic.FileIO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class Car
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CarId { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public int FuelTypeId { get; set; }
        public int CarStatusId { get; set; }
        public int LocationId { get; set; }

        // Nullable: null = added by Admin, set = added by Agent
        public int? AgentId { get; set; }

        [Required]
        [StringLength(100)]
        public string Model { get; set; } = string.Empty;
        public int? CarYear { get; set; }

        [StringLength(50)]
        public string? Color { get; set; }
        public int? NoSeats { get; set; }

        [StringLength(50)]
        public string? Transmission { get; set; }

        [StringLength(50)]
        public string? Mileage { get; set; }
        public decimal? PricePerDay { get; set; }
       
        public decimal? PricePerHour { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }
        public DateTime? CreatedAt { get; set; }

        [ForeignKey("BrandId")]
        public CarBrand? Brand { get; set; }

        [ForeignKey("CategoryId")]
        public CarCategory? Category { get; set; }

        [ForeignKey("FuelTypeId")]
        public FuelType? FuelType { get; set; }

        [ForeignKey("CarStatusId")]
        public CarStatus? CarStatus { get; set; }

        [ForeignKey("LocationId")]
        public Location? Location { get; set; }

        [ForeignKey("AgentId")]
        public User? Agent { get; set; }

        public List<CarImage> CarImages { get; set; } = new();
    }
}