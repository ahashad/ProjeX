using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Application.Invoice.Commands
{
    public class PlanInvoiceCommand
    {
        public Guid ProjectId { get; set; }
        public DateTime InvoiceDate { get; set; } = DateTime.Today;
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);
        public decimal TaxRate { get; set; } = 0.10m; // 10% default tax rate
        public string Notes { get; set; } = string.Empty;
        public List<PlanInvoiceLineItem> LineItems { get; set; } = new List<PlanInvoiceLineItem>();
    }

    public class PlanInvoiceLineItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public LineItemType LineItemType { get; set; } = LineItemType.Labor;
    }
}

