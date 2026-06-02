namespace CarRentalSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 public class CheckinDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CheckinDetailsId { get; set; }

        public int ReservationId { get; set; }

        public int MileageIn { get; set; }

        public int FuelIn { get; set; }

        [StringLength(1000)]
        public string? Damages { get; set; }

        [Required]
        [StringLength(100)]
        public string AgentName { get; set; } = string.Empty;

        public DateTime CompletedAt { get; set; }

        [ForeignKey("ReservationId")]
        public Reservation? Reservation { get; set; }
    }