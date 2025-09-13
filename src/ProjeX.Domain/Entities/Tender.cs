using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Tender : AuditableEntity
    {
        public Guid Id { get; set; }
        public string TenderNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public Guid? AccountId { get; set; }
        public string IssuingOrganization { get; set; } = string.Empty;
        public TenderType TenderType { get; set; }
        public TenderStatus Status { get; set; }
        public DateTime PublishedDate { get; set; }
        public DateTime SubmissionDeadline { get; set; }
        public DateTime? AwardDate { get; set; }
        public decimal EstimatedValue { get; set; }
        public string Currency { get; set; } = "USD";
        public string Description { get; set; } = string.Empty;
        public string Requirements { get; set; } = string.Empty;
        public string EvaluationCriteria { get; set; } = string.Empty;
        public Guid? AssignedToId { get; set; }
        public decimal? SubmittedBidAmount { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public bool IsWon { get; set; }
        public string WinLossReason { get; set; } = string.Empty;
        public Guid? ConvertedProjectId { get; set; }
        public string DocumentPath { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Account? Account { get; set; }
        public virtual Employee? AssignedTo { get; set; }
        public virtual Project? ConvertedProject { get; set; }
        public virtual ICollection<TenderDocument> Documents { get; set; } = new List<TenderDocument>();
        public virtual ICollection<TenderActivity> Activities { get; set; } = new List<TenderActivity>();
    }
}

