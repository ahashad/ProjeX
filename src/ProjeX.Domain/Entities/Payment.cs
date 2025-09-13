using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Payment : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public Invoice Invoice { get; set; } = null!;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public PaymentMethod Method { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; }
    }
}

