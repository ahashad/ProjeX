using LastMinute.Consultancy.Domain.Common;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Domain.Entities
{
    public class Invoice : AuditableEntity
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime? PaidDate { get; set; }
        public string? PaymentReference { get; set; }
        
        // Navigation properties
        public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}

