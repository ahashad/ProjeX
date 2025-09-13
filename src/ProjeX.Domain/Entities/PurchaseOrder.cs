using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class PurchaseOrder : AuditableEntity
    {
        public Guid Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public Guid VendorId { get; set; }
        public Guid? VendorContractId { get; set; }
        public Guid? ProjectId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? RequiredDate { get; set; }
        public PurchaseOrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string Description { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string PaymentTerms { get; set; } = string.Empty;
        public Guid? RequestedById { get; set; }
        public Guid? ApprovedById { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string ApprovalNotes { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Vendor Vendor { get; set; } = null!;
        public virtual VendorContract? VendorContract { get; set; }
        public virtual Project? Project { get; set; }
        public virtual Employee? RequestedBy { get; set; }
        public virtual Employee? ApprovedBy { get; set; }
        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
        public virtual ICollection<GoodsReceipt> GoodsReceipts { get; set; } = new List<GoodsReceipt>();
    }
}

