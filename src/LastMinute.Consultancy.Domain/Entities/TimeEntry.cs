using LastMinute.Consultancy.Domain.Common;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Domain.Entities
{
    public class TimeEntry : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ActualAssignmentId { get; set; }
        public ActualAssignment ActualAssignment { get; set; } = null!;
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public string Description { get; set; } = string.Empty;
        public TimeEntryStatus Status { get; set; }
        public bool IsBillable { get; set; } = true;
        public decimal? BillableRate { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}

