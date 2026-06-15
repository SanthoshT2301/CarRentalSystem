using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Models
{
    [Index(nameof(Email), nameof(Phone), IsUnique = true)]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [StringLength(20)]
        public string? Phone { get; set; }

        [Required]
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        public int RoleId { get; set; }
        public bool? IsActive { get; set; } = true;

        /// <summary>
        /// Customers are auto-approved (true) on register.
        /// Admin/Agent accounts start as false and require an existing Admin to approve them.
        /// </summary>
        public bool IsApproved { get; set; } = false;

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey("RoleId")]
        public Role? Role { get; set; }
    }
}