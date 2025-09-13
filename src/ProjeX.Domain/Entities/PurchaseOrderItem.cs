using ProjeX.Domain.Common;

namespace ProjeX.Domain.Entities
{
    public class PurchaseOrderItem : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid PurchaseOrderId { get; set; }
        public int LineNumber { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual ICollection<GoodsReceiptItem> GoodsReceiptItems { get; set; } = new List<GoodsReceiptItem>();
    }
}

