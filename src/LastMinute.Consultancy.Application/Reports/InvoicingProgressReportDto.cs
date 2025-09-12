namespace LastMinute.Consultancy.Application.Reports
{
    public class InvoicingProgressReportDto
    {
        public List<InvoiceAgingDto> InvoiceAging { get; set; } = new List<InvoiceAgingDto>();
        public DateTime ReportDate { get; set; }
        public decimal TotalInvoiced { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal OverdueAmount { get; set; }
        public InvoicingSummaryDto Summary { get; set; } = new InvoicingSummaryDto();
    }

    public class InvoiceAgingDto
    {
        public Guid InvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string ProjectName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal OutstandingAmount { get; set; }
        public int DaysOverdue { get; set; }
        public string Status { get; set; } = string.Empty;
        public string AgingBucket { get; set; } = string.Empty; // Current, 1-30, 31-60, 61-90, 90+
    }

    public class InvoicingSummaryDto
    {
        public int TotalInvoices { get; set; }
        public int PaidInvoices { get; set; }
        public int OverdueInvoices { get; set; }
        public decimal Current { get; set; } // Not yet due
        public decimal Days1To30 { get; set; }
        public decimal Days31To60 { get; set; }
        public decimal Days61To90 { get; set; }
        public decimal Days90Plus { get; set; }
        public decimal AveragePaymentDays { get; set; }
        public decimal CollectionRate { get; set; } // Percentage of invoices paid
    }
}

