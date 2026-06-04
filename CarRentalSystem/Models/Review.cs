using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewId { get; set; }
        public int UserId { get; set; }
        public int ReservationId { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDisputed { get; set; } = false;

        [StringLength(1000)]
        public string? DisputeResolution { get; set; }

        // Navigation Properties
        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("ReservationId")]
        public Reservation? Reservation { get; set; }
    }
}
