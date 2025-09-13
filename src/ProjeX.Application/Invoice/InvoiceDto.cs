namespace ProjeX.Application.Invoice
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public Guid ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public List<InvoiceLineItemDto> LineItems { get; set; } = new List<InvoiceLineItemDto>();
    }

    public class InvoiceLineItemDto
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public string LineItemType { get; set; } = string.Empty;
    }
}

