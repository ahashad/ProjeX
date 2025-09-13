using ProjeX.Domain.Enums;

namespace ProjeX.Application.VendorManagement
{
    public class VendorDto
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
    public string VendorCode { get; set; } = string.Empty;
   public VendorStatus Status { get; set; }
 public VendorCategory Category { get; set; }
     public string Email { get; set; } = string.Empty;
      public string Phone { get; set; } = string.Empty;
     public string Address { get; set; } = string.Empty;
        public bool IsActive { get; set; }
      public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
    }

    public class CreateVendorRequest
    {
        public string CompanyName { get; set; } = string.Empty;
    public VendorCategory Category { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class UpdateVendorRequest
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public VendorCategory Category { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }

    public class ThreeWayMatchResult
    {
        public Guid VendorInvoiceId { get; set; }
  public bool IsMatched { get; set; }
  public List<string> Issues { get; set; } = new();
        public List<MatchingDetail> MatchingDetails { get; set; } = new();
    }

    public class MatchingDetail
    {
        public int LineNumber { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal POQuantity { get; set; }
        public decimal POUnitPrice { get; set; }
   public decimal POLineTotal { get; set; }
        public decimal GRQuantity { get; set; }
        public decimal InvoiceQuantity { get; set; }
        public decimal InvoiceUnitPrice { get; set; }
   public decimal InvoiceLineTotal { get; set; }
        public bool IsMatched { get; set; }
        public List<string> Issues { get; set; } = new();
    }

    public class VendorPerformanceDto
    {
        public Guid VendorId { get; set; }
 public string VendorName { get; set; } = string.Empty;
      public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalOrderValue { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
        public int TotalInvoices { get; set; }
        public decimal InvoiceMatchRate { get; set; }
        public double AveragePaymentDays { get; set; }
    }
}