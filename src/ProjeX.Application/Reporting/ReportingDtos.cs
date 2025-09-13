namespace ProjeX.Application.Reporting
{
    public class TrendDataDto
    {
        public DateTime Period { get; set; }
    public decimal Value { get; set; }
        public string Label { get; set; } = string.Empty;
     public string Category { get; set; } = string.Empty;
    }

    public class MonthlyFinancialDto
 {
        public int Year { get; set; }
public int Month { get; set; }
   public string MonthName { get; set; } = string.Empty;
 public decimal Revenue { get; set; }
        public decimal Costs { get; set; }
        public decimal Expenses { get; set; }
      public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
        public int ProjectsCompleted { get; set; }
        public int InvoicesIssued { get; set; }
        public decimal BillableHours { get; set; }
    }

  public class ExecutiveDashboardDto
    {
    public int TotalActiveProjects { get; set; }
        public decimal TotalContractValue { get; set; }
        public decimal TotalBudgetAllocated { get; set; }
        public decimal TotalBudgetSpent { get; set; }
        public decimal ResourceUtilizationPercent { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal ProjectProfitability { get; set; }
        public decimal SalesPipelineValue { get; set; }
     public decimal WeightedPipelineValue { get; set; }
        public decimal OnTimeDeliveryRate { get; set; }
 public decimal ClientSatisfactionScore { get; set; }
        public List<TrendDataDto> EmployeeUtilizationTrend { get; set; } = new();
    }

   public class ProjectPerformanceReportDto
    {
    public string ReportPeriod { get; set; } = string.Empty;
      public int TotalProjects { get; set; }
        public int OnTrackProjects { get; set; }
        public int OverBudgetProjects { get; set; }
 public int DelayedProjects { get; set; }
           public decimal AverageBudgetVariance { get; set; }
        public List<ProjectPerformanceDto> ProjectDetails { get; set; } = new();
    }

    public class ProjectPerformanceDto
    {
    public Guid ProjectId { get; set; }
      public string ProjectName { get; set; } = string.Empty;
  public string ClientName { get; set; } = string.Empty;
   public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal ContractValue { get; set; }
        public decimal BudgetAllocated { get; set; }
        public decimal BudgetSpent { get; set; }
      public decimal BudgetVariance { get; set; }
        public decimal BudgetVariancePercent { get; set; }
     public int ScheduleVarianceDays { get; set; }
 public int ProgressPercent { get; set; }
        public int TeamSize { get; set; }
        public bool IsOnTrack { get; set; }
    }

    public class ResourceUtilizationReportDto
    {
        public string ReportPeriod { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
      public decimal AverageUtilization { get; set; }
      public int OverAllocatedCount { get; set; }
        public int UnderUtilizedCount { get; set; }
        public int OptimalUtilizationCount { get; set; }
        public List<EmployeeUtilizationDto> EmployeeUtilization { get; set; } = new();
    }

    public class EmployeeUtilizationDto
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public decimal TotalAllocationPercent { get; set; }
        public int ProjectCount { get; set; }
  public bool IsOverAllocated { get; set; }
        public bool IsUnderUtilized { get; set; }
        public string UtilizationStatus { get; set; } = string.Empty;
        public List<ProjectAssignmentDto> Projects { get; set; } = new();
  }

    public class ProjectAssignmentDto
    {
        public string ProjectName { get; set; } = string.Empty;
        public decimal AllocationPercent { get; set; }
    public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
  }

    public class FinancialReportDto
    {
  public string ReportPeriod { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
     public decimal PendingRevenue { get; set; }
   public decimal OverdueRevenue { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal PendingExpenses { get; set; }
     public decimal TotalBudgetAllocated { get; set; }
        public decimal TotalBudgetSpent { get; set; }
   public decimal BudgetUtilizationPercent { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossProfitMargin { get; set; }
        public List<MonthlyFinancialDto> MonthlyBreakdown { get; set; } = new();
    }

    public class ChartDataDto
    {
   public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
     public string Color { get; set; } = string.Empty;
    }

 public class ProjectFinancialSummaryDto
    {
        public Guid ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal BudgetedAmount { get; set; }
        public decimal ActualCosts { get; set; }
  public decimal InvoicedAmount { get; set; }
      public decimal PaidAmount { get; set; }
   public decimal ProfitLoss { get; set; }
        public decimal ProfitMargin { get; set; }
      public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class EmployeePerformanceDto
    {
     public Guid EmployeeId { get; set; }
      public string EmployeeName { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public decimal BillableHours { get; set; }
   public decimal NonBillableHours { get; set; }
      public decimal UtilizationRate { get; set; }
        public decimal Revenue { get; set; }
        public decimal RevenuePerHour { get; set; }
        public int ProjectsWorked { get; set; }
        public int TasksCompleted { get; set; }
 public decimal AverageTaskRating { get; set; }
    }
}