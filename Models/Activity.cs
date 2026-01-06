using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM___Customer_Relationship_Management_System.Models
{
    public class Activity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Activity Type")]
        public ActivityType Type { get; set; }

        [Display(Name = "Entity Type")]
        public EntityType EntityType { get; set; }

        [Display(Name = "Entity ID")]
        public int EntityId { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign keys
        [Required]
        [Display(Name = "Performed By")]
        public string PerformedById { get; set; } = string.Empty;

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }

        [Display(Name = "Deal")]
        public int? DealId { get; set; }

        // Navigation properties
        [ForeignKey("PerformedById")]
        public virtual ApplicationUser? PerformedBy { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("LeadId")]
        public virtual Lead? Lead { get; set; }

        [ForeignKey("DealId")]
        public virtual Deal? Deal { get; set; }
    }

    public enum ActivityType
    {
        Created,
        Updated,
        Deleted,
        StatusChanged,
        Assigned,
        [Display(Name = "Note Added")]
        NoteAdded,
        [Display(Name = "Task Created")]
        TaskCreated,
        [Display(Name = "Task Completed")]
        TaskCompleted,
        [Display(Name = "Deal Won")]
        DealWon,
        [Display(Name = "Deal Lost")]
        DealLost,
        Converted,
        [Display(Name = "Email Sent")]
        EmailSent,
        [Display(Name = "Call Made")]
        CallMade,
        [Display(Name = "Meeting Scheduled")]
        MeetingScheduled
    }

    public enum EntityType
    {
        Customer,
        Contact,
        Lead,
        Deal,
        Task,
        Note
    }
}
