using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class VendorInvoice : AuditableEntity
    {
        public Guid Id { get; set; }
        public string VendorInvoiceNumber { get; set; } = string.Empty;
        public Guid VendorId { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public Guid? GoodsReceiptId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public VendorInvoiceStatus Status { get; set; }
        public string Description { get; set; } = string.Empty;
        public string PaymentReference { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public decimal? PaymentAmount { get; set; }
        public Guid? ProcessedById { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public bool IsThreeWayMatched { get; set; }
        public string MatchingNotes { get; set; } = string.Empty;
        public string DocumentPath { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Vendor Vendor { get; set; } = null!;
        public virtual PurchaseOrder? PurchaseOrder { get; set; }
        public virtual GoodsReceipt? GoodsReceipt { get; set; }
        public virtual Employee? ProcessedBy { get; set; }
        public virtual ICollection<VendorInvoiceItem> Items { get; set; } = new List<VendorInvoiceItem>();
    }
}

