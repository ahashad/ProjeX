using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Vendor : AuditableEntity
    {
        public Guid Id { get; set; }
        public string VendorCode { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public VendorStatus Status { get; set; }
        public VendorCategory Category { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public string Currency { get; set; } = "USD";
        public decimal CreditLimit { get; set; }
        public decimal CurrentBalance { get; set; }
        public DateTime? LastPaymentDate { get; set; }
        public string BankDetails { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual ICollection<VendorContract> Contracts { get; set; } = new List<VendorContract>();
        public virtual ICollection<PurchaseOrder> PurchaseOrders { get; set; } = new List<PurchaseOrder>();
        public virtual ICollection<VendorInvoice> VendorInvoices { get; set; } = new List<VendorInvoice>();
    }
}

