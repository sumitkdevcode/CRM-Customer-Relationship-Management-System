using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM___Customer_Relationship_Management_System.Models
{
    public class CrmTask
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Display(Name = "Task Type")]
        public TaskType Type { get; set; } = TaskType.Call;

        [Display(Name = "Priority")]
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        [Display(Name = "Status")]
        public TaskStatus Status { get; set; } = TaskStatus.Pending;

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime DueDate { get; set; }

        [Display(Name = "Completed Date")]
        [DataType(DataType.DateTime)]
        public DateTime? CompletedDate { get; set; }

        [Display(Name = "Reminder")]
        [DataType(DataType.DateTime)]
        public DateTime? ReminderDate { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Required]
        [Display(Name = "Assigned To")]
        public string AssignedToId { get; set; } = string.Empty;

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Contact")]
        public int? ContactId { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }

        [Display(Name = "Deal")]
        public int? DealId { get; set; }

        // Navigation properties
        [ForeignKey("AssignedToId")]
        public virtual ApplicationUser? AssignedTo { get; set; }

        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("ContactId")]
        public virtual Contact? Contact { get; set; }

        [ForeignKey("LeadId")]
        public virtual Lead? Lead { get; set; }

        [ForeignKey("DealId")]
        public virtual Deal? Deal { get; set; }

        // Computed property
        [NotMapped]
        public bool IsOverdue => Status != TaskStatus.Completed && DueDate < DateTime.UtcNow;
    }

    public enum TaskType
    {
        Call,
        Email,
        Meeting,
        [Display(Name = "Follow Up")]
        FollowUp,
        Demo,
        Presentation,
        Research,
        Other
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public enum TaskStatus
    {
        Pending,
        [Display(Name = "In Progress")]
        InProgress,
        Completed,
        Cancelled,
        Deferred
    }
}
