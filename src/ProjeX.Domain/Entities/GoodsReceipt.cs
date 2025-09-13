using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class GoodsReceipt : AuditableEntity
    {
        public Guid Id { get; set; }
        public string GRNumber { get; set; } = string.Empty;
        public Guid PurchaseOrderId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public Guid ReceivedById { get; set; }
        public GoodsReceiptStatus Status { get; set; }
        public string DeliveryNote { get; set; } = string.Empty;
        public string ReceivedCondition { get; set; } = string.Empty;
        public bool IsPartialReceipt { get; set; }
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;
        public virtual Employee ReceivedBy { get; set; } = null!;
        public virtual ICollection<GoodsReceiptItem> Items { get; set; } = new List<GoodsReceiptItem>();
    }
}

