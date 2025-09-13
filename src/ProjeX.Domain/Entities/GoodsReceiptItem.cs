using ProjeX.Domain.Common;

namespace ProjeX.Domain.Entities
{
    public class GoodsReceiptItem : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid GoodsReceiptId { get; set; }
        public Guid PurchaseOrderItemId { get; set; }
        public int LineNumber { get; set; }
        public decimal OrderedQuantity { get; set; }
        public decimal ReceivedQuantity { get; set; }
        public decimal RejectedQuantity { get; set; }
        public string RejectionReason { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual GoodsReceipt GoodsReceipt { get; set; } = null!;
        public virtual PurchaseOrderItem PurchaseOrderItem { get; set; } = null!;
    }
}

