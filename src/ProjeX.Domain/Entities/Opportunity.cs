using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Opportunity : AuditableEntity
    {
        public Guid Id { get; set; }
        public string OpportunityNumber { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid AccountId { get; set; }
        public Guid? PrimaryContactId { get; set; }
        public Guid? OwnerId { get; set; }
        public OpportunityStage Stage { get; set; }
        public decimal EstimatedValue { get; set; }
        public string Currency { get; set; } = "USD";
        public int ProbabilityPercent { get; set; }
        public DateTime? ExpectedCloseDate { get; set; }
        public DateTime? ActualCloseDate { get; set; }
        public OpportunitySource Source { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string CompetitorInfo { get; set; } = string.Empty;
        public string NextSteps { get; set; } = string.Empty;
        public DateTime? LastActivityDate { get; set; }
        public string LossReason { get; set; } = string.Empty;
        public Guid? ConvertedProjectId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Account Account { get; set; } = null!;
        public virtual Contact? PrimaryContact { get; set; }
        public virtual Employee? Owner { get; set; }
        public virtual Project? ConvertedProject { get; set; }
        public virtual ICollection<OpportunityActivity> Activities { get; set; } = new List<OpportunityActivity>();
    }
}

