using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models;
public class MaintenanceAlert
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MaintenanceAlertId { get; set; }

        public int CarId { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Priority { get; set; } = "Medium"; 

        [Required]
        [StringLength(100)]
        public string ReportedBy { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Reported"; 

        public DateTime CreatedAt { get; set; } 

        // Navigation properties
        [ForeignKey("CarId")]
        public Car? Car { get; set; }
    }