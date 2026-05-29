using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class Payment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PaymentId { get; set; }
        public int ReservationId { get; set; }
        public int PaymentMethodId { get; set; }
        public int PaymentStatusId { get; set; }

        [StringLength(100)]
        public string? TransactionId { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? PaymentDate { get; set; } 

        [ForeignKey("ReservationId")]
        public Reservation? Reservation { get; set; }

        [ForeignKey("PaymentMethodId")]
        public PaymentMethod? PaymentMethod { get; set; }

        [ForeignKey("PaymentStatusId")]
        public PaymentStatus? PaymentStatus { get; set; }
    }
}
