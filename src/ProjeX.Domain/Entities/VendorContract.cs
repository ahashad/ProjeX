using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class VendorContract : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid VendorId { get; set; }
        public string ContractNumber { get; set; } = string.Empty;
        public string ContractName { get; set; } = string.Empty;
        public ContractType ContractType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ContractValue { get; set; }
        public string Currency { get; set; } = "USD";
        public ContractStatus Status { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public string DeliveryTerms { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Terms { get; set; } = string.Empty;
        public Guid? ProjectId { get; set; }
        public bool AutoRenewal { get; set; }
        public int? RenewalPeriodMonths { get; set; }
        public DateTime? NoticeDate { get; set; }
        public string DocumentPath { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;

        // Navigation properties
        public virtual Vendor Vendor { get; set; } = null!;
        public virtual Project? Project { get; set; }
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
    }
}

