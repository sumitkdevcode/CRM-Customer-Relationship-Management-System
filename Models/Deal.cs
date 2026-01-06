using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM___Customer_Relationship_Management_System.Models
{
    public class Deal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Deal Title")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Deal Value")]
        public decimal Value { get; set; }

        [Display(Name = "Deal Stage")]
        public DealStage Stage { get; set; } = DealStage.Qualification;

        [Range(0, 100)]
        [Display(Name = "Probability (%)")]
        public int Probability { get; set; } = 10;

        [Display(Name = "Expected Close Date")]
        [DataType(DataType.Date)]
        public DateTime? ExpectedCloseDate { get; set; }

        [Display(Name = "Actual Close Date")]
        [DataType(DataType.Date)]
        public DateTime? ActualCloseDate { get; set; }

        [Display(Name = "Deal Type")]
        public DealType Type { get; set; } = DealType.NewBusiness;

        [StringLength(100)]
        [Display(Name = "Product/Service")]
        public string? Product { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime? UpdatedAt { get; set; }

        // Foreign keys
        [Required]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        [Display(Name = "Assigned To")]
        public string? AssignedToId { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer { get; set; }

        [ForeignKey("AssignedToId")]
        public virtual ApplicationUser? AssignedTo { get; set; }

        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
        public virtual ICollection<CrmTask> Tasks { get; set; } = new List<CrmTask>();
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();

        // Computed property
        [NotMapped]
        public decimal WeightedValue => Value * Probability / 100;
    }

    public enum DealStage
    {
        Qualification,
        [Display(Name = "Needs Analysis")]
        NeedsAnalysis,
        [Display(Name = "Value Proposition")]
        ValueProposition,
        [Display(Name = "Decision Makers")]
        DecisionMakers,
        Proposal,
        Negotiation,
        [Display(Name = "Closed Won")]
        ClosedWon,
        [Display(Name = "Closed Lost")]
        ClosedLost
    }

    public enum DealType
    {
        [Display(Name = "New Business")]
        NewBusiness,
        [Display(Name = "Existing Business")]
        ExistingBusiness,
        Renewal,
        Upsell,
        [Display(Name = "Cross-Sell")]
        CrossSell
    }
}
