using ProjeX.Domain.Common;

namespace ProjeX.Domain.Entities
{
    public class VendorInvoiceItem : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid VendorInvoiceId { get; set; }
        public int LineNumber { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public Guid? PurchaseOrderItemId { get; set; }
        public bool IsMatched { get; set; }
        public string MatchingNotes { get; set; } = string.Empty;

        // Navigation properties
        public virtual VendorInvoice VendorInvoice { get; set; } = null!;
        public virtual PurchaseOrderItem? PurchaseOrderItem { get; set; }
    }
}

