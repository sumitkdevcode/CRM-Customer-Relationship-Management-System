using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CRM___Customer_Relationship_Management_System.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? ProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        public string FullName => $"{FirstName} {LastName}";

        // Navigation properties
        public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
        public virtual ICollection<Lead> Leads { get; set; } = new List<Lead>();
        public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
        public virtual ICollection<CrmTask> Tasks { get; set; } = new List<CrmTask>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }
}
