namespace LastMinute.Consultancy.Application.Invoice.Commands
{
    public class ConfirmInvoiceCommand
    {
        public Guid InvoiceId { get; set; }
        public string ConfirmationNotes { get; set; } = string.Empty;
    }
}

