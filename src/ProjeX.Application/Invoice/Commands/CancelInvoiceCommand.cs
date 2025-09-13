namespace ProjeX.Application.Invoice.Commands
{
    public class CancelInvoiceCommand
    {
        public Guid InvoiceId { get; set; }
        public string CancellationReason { get; set; } = string.Empty;
    }
}

