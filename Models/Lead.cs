using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM___Customer_Relationship_Management_System.Models
{
    public class Lead
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

        [Required]
        [StringLength(200)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Phone { get; set; }

        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string? CompanyName { get; set; }

        [StringLength(100)]
        [Display(Name = "Job Title")]
        public string? JobTitle { get; set; }

        [StringLength(100)]
        public string? Industry { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [Display(Name = "Lead Source")]
        public LeadSource Source { get; set; } = LeadSource.Website;

        [Display(Name = "Lead Status")]
        public LeadStatus Status { get; set; } = LeadStatus.New;

        [Range(1, 5)]
        [Display(Name = "Lead Rating (1-5)")]
        public int Rating { get; set; } = 3;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Estimated Value")]
        public decimal? EstimatedValue { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Converted At")]
        public DateTime? ConvertedAt { get; set; }

        // Foreign keys
        [Display(Name = "Assigned To")]
        public string? AssignedToId { get; set; }

        [Display(Name = "Converted Customer")]
        public int? ConvertedCustomerId { get; set; }

        // Navigation properties
        [ForeignKey("AssignedToId")]
        public virtual ApplicationUser? AssignedTo { get; set; }

        [ForeignKey("ConvertedCustomerId")]
        public virtual Customer? ConvertedCustomer { get; set; }

        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<CrmTask> Tasks { get; set; } = new List<CrmTask>();
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

        // Computed property
        [NotMapped]
        public string FullName => $"{FirstName} {LastName}";
    }

    public enum LeadSource
    {
        Website,
        Referral,
        [Display(Name = "Social Media")]
        SocialMedia,
        Advertisement,
        [Display(Name = "Cold Call")]
        ColdCall,
        [Display(Name = "Trade Show")]
        TradeShow,
        [Display(Name = "Email Campaign")]
        EmailCampaign,
        Partner,
        Other
    }

    public enum LeadStatus
    {
        New,
        Contacted,
        Qualified,
        Unqualified,
        Nurturing,
        Converted,
        Lost
    }
}
