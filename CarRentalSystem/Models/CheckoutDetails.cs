namespace CarRentalSystem.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
 public class CheckoutDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CheckoutDetailsId { get; set; }

        public int ReservationId { get; set; }

        [Required]
        [StringLength(100)]
        public string DriverLicense { get; set; } = string.Empty;

        public int MileageOut { get; set; }

        public int FuelOut { get; set; }

        [Required]
        [StringLength(100)]
        public string AgentName { get; set; } = string.Empty;

        public DateTime CompletedAt { get; set; }

        [ForeignKey("ReservationId")]
        public Reservation? Reservation { get; set; }
    }