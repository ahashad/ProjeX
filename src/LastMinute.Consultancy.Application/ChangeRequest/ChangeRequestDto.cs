namespace LastMinute.Consultancy.Application.ChangeRequest
{
    public class ChangeRequestDto
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ChangeRequestType { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public int EstimatedHours { get; set; }
        public DateTime RequestedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string RequestedBy { get; set; } = string.Empty;
        public string ApprovedBy { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
        public string ApprovalNotes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
    }
}

