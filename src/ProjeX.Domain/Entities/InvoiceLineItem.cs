using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class InvoiceLineItem : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;
        public Guid? TimeEntryId { get; set; }
        public TimeEntry? TimeEntry { get; set; }
        public Guid? OverheadId { get; set; }
        public Overhead? Overhead { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitRate { get; set; }
        public decimal Amount { get; set; }
        public LineItemType Type { get; set; }
    }
}

