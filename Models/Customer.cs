using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM___Customer_Relationship_Management_System.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Industry")]
        public string? Industry { get; set; }

        [StringLength(50)]
        [Display(Name = "Company Size")]
        public string? CompanySize { get; set; }

        [StringLength(500)]
        public string? Website { get; set; }

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(20)]
        [Display(Name = "Postal Code")]
        public string? PostalCode { get; set; }

        [Display(Name = "Customer Type")]
        public CustomerType CustomerType { get; set; } = CustomerType.Prospect;

        [Display(Name = "Customer Status")]
        public CustomerStatus Status { get; set; } = CustomerStatus.Active;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Annual Revenue")]
        public decimal? AnnualRevenue { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Display(Name = "Assigned To")]
        public string? AssignedToId { get; set; }

        // Navigation properties
        [ForeignKey("AssignedToId")]
        public virtual ApplicationUser? AssignedTo { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<CrmTask> Tasks { get; set; } = new List<CrmTask>();
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
    }

    public enum CustomerType
    {
        Prospect,
        Lead,
        Customer,
        Partner,
        Vendor
    }

    public enum CustomerStatus
    {
        Active,
        Inactive,
        Suspended,
        Churned
    }
}
