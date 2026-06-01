using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class Reservation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReservationId { get; set; }
        public int UserId { get; set; }
        public int CarId { get; set; }
        public int PickupLocationId { get; set; }
        public int DropoffLocationId { get; set; }
        public int ReservationStatusId { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public DateTime DropDate { get; set; }
        public decimal? TotalAmount { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }
        public bool IsHourly { get; set; } = false;
        public int DurationHours { get; set; } = 0;

        [StringLength(50)]
        public string? PickupTime { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("CarId")]
        public Car? Car { get; set; }

        [ForeignKey("PickupLocationId")]
        public Location? PickupLocation { get; set; }

        [ForeignKey("DropoffLocationId")]
        public Location? DropoffLocation { get; set; }

        [ForeignKey("ReservationStatusId")]
        public ReservationStatus? ReservationStatus { get; set; }
    }
}
