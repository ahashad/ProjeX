using ProjeX.Domain.Enums;

namespace ProjeX.Application.Payment.Commands
{
    public class RecordPaymentCommand
    {
        public Guid InvoiceId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Today;
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}

