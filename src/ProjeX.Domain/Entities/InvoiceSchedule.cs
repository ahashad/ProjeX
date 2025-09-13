using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class InvoiceSchedule : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid InvoicePlanId { get; set; }
        public int SequenceNumber { get; set; }
        public DateTime ScheduledDate { get; set; }
        public decimal ScheduledAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public Guid? MilestoneId { get; set; }
        public InvoiceScheduleStatus Status { get; set; }
        public Guid? GeneratedInvoiceId { get; set; }
        public DateTime? ActualInvoiceDate { get; set; }
        public decimal? ActualAmount { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual InvoicePlan InvoicePlan { get; set; } = null!;
        public virtual Deliverable? Milestone { get; set; }
        public virtual Invoice? GeneratedInvoice { get; set; }
    }
}

