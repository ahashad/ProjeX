using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class ChangeRequest : AuditableEntity
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string BusinessJustification { get; set; } = string.Empty;
        public ChangeRequestType Type { get; set; }
        public ChangeRequestPriority Priority { get; set; }
        public ChangeRequestStatus Status { get; set; }
        public decimal EstimatedCost { get; set; }
        public decimal EstimatedHours { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string? ReviewedBy { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string? ReviewComments { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovalComments { get; set; }
        public DateTime? ImplementedDate { get; set; }
        public decimal? ActualCost { get; set; }
        public decimal? ActualHours { get; set; }
    }
}

