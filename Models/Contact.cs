using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM___Customer_Relationship_Management_System.Models
{
    public class Contact
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(50)]
        [Display(Name = "Mobile Phone")]
        public string? Mobile { get; set; }

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

        [Display(Name = "Is Primary Contact")]
        public bool IsPrimary { get; set; } = false;

        [Display(Name = "Contact Status")]
        public ContactStatus Status { get; set; } = ContactStatus.Active;

        [StringLength(500)]
        [Display(Name = "LinkedIn Profile")]
        public string? LinkedInProfile { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        public virtual ICollection<CrmTask> Tasks { get; set; } = new List<CrmTask>();
        public virtual ICollection<Note> ContactNotes { get; set; } = new List<Note>();

        // Computed property
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }

    public enum ContactStatus
    {
        Active,
        Inactive,
        DoNotContact
    }
}
