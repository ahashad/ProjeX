using ProjeX.Domain.Enums;

namespace ProjeX.Application.Governance
{
    public class ApprovalRequestDto
    {
        public Guid Id { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
       public string Description { get; set; } = string.Empty;
        public Guid RequesterId { get; set; }
  public string RequesterName { get; set; } = string.Empty;
        public ApprovalStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public Guid? ApproverId { get; set; }
        public string ApproverName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
 public string Notes { get; set; } = string.Empty;
        public int Priority { get; set; }
        public decimal AmountRequested { get; set; }
        public string Currency { get; set; } = string.Empty;
    }

    public class BudgetEncumbranceDto
    {
        public Guid ProjectId { get; set; }
public string ProjectName { get; set; } = string.Empty;
        public Guid BudgetId { get; set; }
        public string BudgetDescription { get; set; } = string.Empty;
      public decimal BudgetAmount { get; set; }
 public decimal EncumberedAmount { get; set; }
   public decimal AvailableAmount { get; set; }
        public decimal UtilizationRate { get; set; }
        public DateTime PeriodStart { get; set; }
  public DateTime PeriodEnd { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ChangeOrderDto
    {
      public Guid Id { get; set; }
        public string ChangeOrderNumber { get; set; } = string.Empty;
 public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
public string Description { get; set; } = string.Empty;
        public ChangeRequestType Type { get; set; }
        public string TypeName { get; set; } = string.Empty;
     public ChangeRequestStatus Status { get; set; }
   public string StatusName { get; set; } = string.Empty;
        public decimal EstimatedCost { get; set; }
        public decimal ActualCost { get; set; }
        public DateTime RequestDate { get; set; }
   public DateTime? ApprovalDate { get; set; }
  public DateTime? ImplementationDate { get; set; }
        public Guid RequesterId { get; set; }
        public string RequesterName { get; set; } = string.Empty;
        public Guid? ApproverId { get; set; }
        public string ApproverName { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
        public string Impact { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public class ComplianceReportDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
    public DateTime ReportDate { get; set; }
      public List<ComplianceItemDto> ComplianceItems { get; set; } = new();
public decimal OverallScore { get; set; }
        public string OverallStatus { get; set; } = string.Empty;
     public List<string> Recommendations { get; set; } = new();
    }

    public class ComplianceItemDto
    {
        public string Category { get; set; } = string.Empty;
        public string Item { get; set; } = string.Empty;
      public string Status { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public string Notes { get; set; } = string.Empty;
      public bool IsCompliant { get; set; }
        public DateTime LastChecked { get; set; }
    }
}