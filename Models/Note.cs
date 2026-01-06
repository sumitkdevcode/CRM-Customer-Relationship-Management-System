using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM___Customer_Relationship_Management_System.Models
{
    public class Note
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Note Type")]
        public NoteType Type { get; set; } = NoteType.General;

        [Display(Name = "Is Pinned")]
        public bool IsPinned { get; set; } = false;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Required]
        [Display(Name = "Created By")]
        public string CreatedById { get; set; } = string.Empty;

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Contact")]
        public int? ContactId { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }

        [Display(Name = "Deal")]
        public int? DealId { get; set; }

        // Navigation properties
        [ForeignKey("CreatedById")]
        public virtual ApplicationUser? CreatedBy { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("ContactId")]
        public virtual Contact? Contact { get; set; }

        [ForeignKey("LeadId")]
        public virtual Lead? Lead { get; set; }

        [ForeignKey("DealId")]
        public virtual Deal? Deal { get; set; }
    }

    public enum NoteType
    {
        General,
        [Display(Name = "Meeting Notes")]
        MeetingNotes,
        [Display(Name = "Phone Call")]
        PhoneCall,
        Email,
        Important,
        [Display(Name = "Follow Up")]
        FollowUp,
        Idea
    }
}
