using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Enums;
using ProjeX.Infrastructure.Data;

namespace ProjeX.Application.Reporting
{
    public class EnhancedReportService : IEnhancedReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EnhancedReportService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync()
        {
            var dashboard = new ExecutiveDashboardDto();

            // Financial Metrics
            var activeProjects = await _context.Projects
                .Where(p => p.Status == ProjectStatus.Active)
                .ToListAsync();

            dashboard.TotalActiveProjects = activeProjects.Count;
            dashboard.TotalContractValue = activeProjects.Sum(p => p.ContractValue);
            dashboard.TotalBudgetAllocated = await _context.Budgets
                .Where(b => b.Status == BudgetStatus.Approved)
                .SumAsync(b => b.AllocatedAmount);
            dashboard.TotalBudgetSpent = await _context.Budgets
                .SumAsync(b => b.SpentAmount);

            // Resource Utilization
            var totalEmployees = await _context.Employees.CountAsync(e => e.IsActive);
            var totalCapacity = totalEmployees * 100; // 100% per employee
            var currentUtilization = await _context.ActualAssignments
                .Where(a => a.Status == AssignmentStatus.Active)
                .SumAsync(a => a.AllocationPercent);

            dashboard.ResourceUtilizationPercent = totalCapacity > 0 ? (currentUtilization / totalCapacity) * 100 : 0;

            // Revenue and Profitability
            dashboard.MonthlyRevenue = await CalculateMonthlyRevenueAsync();
            dashboard.ProjectProfitability = await CalculateProjectProfitabilityAsync();

            // Pipeline Metrics
            var opportunities = await _context.Opportunities
                .Where(o => o.IsActive)
                .ToListAsync();

            dashboard.SalesPipelineValue = opportunities.Sum(o => o.EstimatedValue);
            dashboard.WeightedPipelineValue = opportunities.Sum(o => o.EstimatedValue * o.ProbabilityPercent / 100);

            // Performance Indicators
            dashboard.OnTimeDeliveryRate = await CalculateOnTimeDeliveryRateAsync();
            dashboard.ClientSatisfactionScore = await CalculateClientSatisfactionAsync();
            dashboard.EmployeeUtilizationTrend = await GetUtilizationTrendAsync();

            return dashboard;
        }

        public async Task<ProjectPerformanceReportDto> GetProjectPerformanceReportAsync(DateTime fromDate, DateTime toDate)
        {
            var projects = await _context.Projects
                .Include(p => p.Client)
                .Include(p => p.Budgets)
                .Include(p => p.ActualAssignments)
                .Include(p => p.Deliverables)
                .Where(p => p.StartDate >= fromDate && p.StartDate <= toDate)
                .ToListAsync();

            var report = new ProjectPerformanceReportDto
            {
                ReportPeriod = $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                TotalProjects = projects.Count,
                ProjectDetails = new List<ProjectPerformanceDto>()
            };

            foreach (var project in projects)
            {
                var budgetAllocated = project.Budgets.Sum(b => b.AllocatedAmount);
                var budgetSpent = project.Budgets.Sum(b => b.SpentAmount);
                var deliverableProgress = project.Deliverables.Any() 
                    ? project.Deliverables.Average(d => d.ProgressPercentage) 
                    : 0;

                var performance = new ProjectPerformanceDto
                {
                    ProjectId = project.Id,
                    ProjectName = project.ProjectName,
                    ClientName = project.Client?.CompanyName ?? "Unknown",
                    Status = project.Status.ToString(),
                    StartDate = project.StartDate,
                    EndDate = project.EndDate,
                    ContractValue = project.ContractValue,
                    BudgetAllocated = budgetAllocated,
                    BudgetSpent = budgetSpent,
                    BudgetVariance = budgetAllocated - budgetSpent,
                    BudgetVariancePercent = budgetAllocated > 0 ? ((budgetAllocated - budgetSpent) / budgetAllocated) * 100 : 0,
                    ScheduleVarianceDays = (project.EndDate - DateTime.UtcNow).Days,
                    ProgressPercent = (int)deliverableProgress,
                    TeamSize = project.ActualAssignments.Count(a => a.Status == AssignmentStatus.Active),
                    IsOnTrack = deliverableProgress >= 80 && budgetSpent <= budgetAllocated * 0.9m
                };

                report.ProjectDetails.Add(performance);
            }

            // Calculate summary metrics
            report.OnTrackProjects = report.ProjectDetails.Count(p => p.IsOnTrack);
            report.OverBudgetProjects = report.ProjectDetails.Count(p => p.BudgetVariance < 0);
            report.DelayedProjects = report.ProjectDetails.Count(p => p.ScheduleVarianceDays < 0);
            report.AverageBudgetVariance = report.ProjectDetails.Average(p => p.BudgetVariancePercent);

            return report;
        }

        public async Task<ResourceUtilizationReportDto> GetResourceUtilizationReportAsync(DateTime fromDate, DateTime toDate)
        {
            var employees = await _context.Employees
                .Include(e => e.Role)
                .Include(e => e.ActualAssignments)
                    .ThenInclude(a => a.Project)
                .Where(e => e.IsActive)
                .ToListAsync();

            var report = new ResourceUtilizationReportDto
            {
                ReportPeriod = $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                TotalEmployees = employees.Count,
                EmployeeUtilization = new List<EmployeeUtilizationDto>()
            };

            foreach (var employee in employees)
            {
                var assignments = employee.ActualAssignments
                    .Where(a => a.Status == AssignmentStatus.Active &&
                               a.StartDate <= toDate &&
                               (a.EndDate == null || a.EndDate >= fromDate))
                    .ToList();

                var totalAllocation = assignments.Sum(a => a.AllocationPercent);
                var projectCount = assignments.Select(a => a.ProjectId).Distinct().Count();

                var utilization = new EmployeeUtilizationDto
                {
                    EmployeeId = employee.Id,
                    EmployeeName = $"{employee.FirstName} {employee.LastName}",
                    RoleName = employee.Role?.Name ?? "Unknown",
                    TotalAllocationPercent = totalAllocation,
                    ProjectCount = projectCount,
                    IsOverAllocated = totalAllocation > 100,
                    IsUnderUtilized = totalAllocation < 80,
                    UtilizationStatus = totalAllocation > 100 ? "Over-allocated" :
                                       totalAllocation < 80 ? "Under-utilized" : "Optimal",
                    Projects = assignments.Select(a => new ProjectAssignmentDto
                    {
                        ProjectName = a.Project?.ProjectName ?? "Unknown",
                        AllocationPercent = a.AllocationPercent,
                        StartDate = a.StartDate,
                        EndDate = a.EndDate
                    }).ToList()
                };

                report.EmployeeUtilization.Add(utilization);
            }

            // Calculate summary metrics
            report.AverageUtilization = report.EmployeeUtilization.Average(e => e.TotalAllocationPercent);
            report.OverAllocatedCount = report.EmployeeUtilization.Count(e => e.IsOverAllocated);
            report.UnderUtilizedCount = report.EmployeeUtilization.Count(e => e.IsUnderUtilized);
            report.OptimalUtilizationCount = report.TotalEmployees - report.OverAllocatedCount - report.UnderUtilizedCount;

            return report;
        }

        public async Task<FinancialReportDto> GetFinancialReportAsync(DateTime fromDate, DateTime toDate)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Where(i => i.InvoiceDate >= fromDate && i.InvoiceDate <= toDate)
                .ToListAsync();

            var vendorInvoices = await _context.VendorInvoices
                .Include(vi => vi.Vendor)
                .Where(vi => vi.InvoiceDate >= fromDate && vi.InvoiceDate <= toDate)
                .ToListAsync();

            var budgets = await _context.Budgets
                .Include(b => b.Project)
                .Where(b => b.CreatedAt >= fromDate && b.CreatedAt <= toDate)
                .ToListAsync();

            var report = new FinancialReportDto
            {
                ReportPeriod = $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                
                // Revenue Metrics
                TotalRevenue = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount),
                PendingRevenue = invoices.Where(i => i.Status == InvoiceStatus.Sent).Sum(i => i.TotalAmount),
                OverdueRevenue = invoices.Where(i => i.Status == InvoiceStatus.Overdue).Sum(i => i.TotalAmount),
                
                // Expense Metrics
                TotalExpenses = vendorInvoices.Where(vi => vi.Status == VendorInvoiceStatus.Paid).Sum(vi => vi.TotalAmount),
                PendingExpenses = vendorInvoices.Where(vi => vi.Status == VendorInvoiceStatus.PendingPayment).Sum(vi => vi.TotalAmount),
                
                // Budget Metrics
                TotalBudgetAllocated = budgets.Sum(b => b.AllocatedAmount),
                TotalBudgetSpent = budgets.Sum(b => b.SpentAmount),
                BudgetUtilizationPercent = budgets.Sum(b => b.AllocatedAmount) > 0 
                    ? (budgets.Sum(b => b.SpentAmount) / budgets.Sum(b => b.AllocatedAmount)) * 100 
                    : 0,
                
                // Profitability
                GrossProfit = invoices.Where(i => i.Status == InvoiceStatus.Paid).Sum(i => i.TotalAmount) - 
                             vendorInvoices.Where(vi => vi.Status == VendorInvoiceStatus.Paid).Sum(vi => vi.TotalAmount),
                
                // Monthly Breakdown
                MonthlyBreakdown = await GetMonthlyFinancialBreakdownAsync(fromDate, toDate)
            };

            report.GrossProfitMargin = report.TotalRevenue > 0 ? (report.GrossProfit / report.TotalRevenue) * 100 : 0;

            return report;
        }

        public async Task<List<ChartDataDto>> GetProjectStatusChartDataAsync()
        {
            var statusCounts = await _context.Projects
                .GroupBy(p => p.Status)
                .Select(g => new ChartDataDto
                {
                    Label = g.Key.ToString(),
                    Value = g.Count(),
                    Color = GetStatusColor(g.Key)
                })
                .ToListAsync();

            return statusCounts;
        }

        public async Task<List<ChartDataDto>> GetRevenueByClientChartDataAsync(int topN = 10)
        {
            var clientRevenue = await _context.Invoices
                .Include(i => i.Client)
                .Where(i => i.Status == InvoiceStatus.Paid)
                .GroupBy(i => new { i.ClientId, i.Client!.CompanyName })
                .Select(g => new ChartDataDto
                {
                    Label = g.Key.CompanyName,
                    Value = (double)g.Sum(i => i.TotalAmount),
                    Color = GenerateRandomColor()
                })
                .OrderByDescending(x => x.Value)
                .Take(topN)
                .ToListAsync();

            return clientRevenue;
        }

        public async Task<List<TrendDataDto>> GetUtilizationTrendAsync(int months = 12)
        {
            var trends = new List<TrendDataDto>();
            var startDate = DateTime.UtcNow.AddMonths(-months);

            for (int i = 0; i < months; i++)
            {
                var monthStart = startDate.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var totalCapacity = await _context.Employees
                    .Where(e => e.IsActive && e.CreatedAt <= monthEnd)
                    .CountAsync() * 100;

                var utilization = await _context.ActualAssignments
                    .Where(a => a.Status == AssignmentStatus.Active &&
                               a.StartDate <= monthEnd &&
                               (a.EndDate == null || a.EndDate >= monthStart))
                    .SumAsync(a => a.AllocationPercent);

                trends.Add(new TrendDataDto
                {
                    Period = monthStart.ToString("yyyy-MM"),
                    Value = totalCapacity > 0 ? (double)((utilization / totalCapacity) * 100) : 0
                });
            }

            return trends;
        }

        private async Task<decimal> CalculateMonthlyRevenueAsync()
        {
            var currentMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var nextMonth = currentMonth.AddMonths(1);

            return await _context.Invoices
                .Where(i => i.InvoiceDate >= currentMonth && 
                           i.InvoiceDate < nextMonth && 
                           i.Status == InvoiceStatus.Paid)
                .SumAsync(i => i.TotalAmount);
        }

        private async Task<decimal> CalculateProjectProfitabilityAsync()
        {
            var activeProjects = await _context.Projects
                .Include(p => p.Budgets)
                .Where(p => p.Status == ProjectStatus.Active)
                .ToListAsync();

            if (!activeProjects.Any()) return 0;

            var totalRevenue = activeProjects.Sum(p => p.ContractValue);
            var totalCosts = activeProjects.SelectMany(p => p.Budgets).Sum(b => b.SpentAmount);

            return totalRevenue > 0 ? ((totalRevenue - totalCosts) / totalRevenue) * 100 : 0;
        }

        private async Task<decimal> CalculateOnTimeDeliveryRateAsync()
        {
            var completedDeliverables = await _context.Deliverables
                .Where(d => d.Status == DeliverableStatus.Completed && d.ActualEndDate.HasValue)
                .ToListAsync();

            if (!completedDeliverables.Any()) return 0;

            var onTimeCount = completedDeliverables.Count(d => d.ActualEndDate <= d.EndDate);
            return ((decimal)onTimeCount / completedDeliverables.Count) * 100;
        }

        private async Task<decimal> CalculateClientSatisfactionAsync()
        {
            // This would typically come from client feedback/surveys
            // For now, return a calculated score based on project success metrics
            var completedProjects = await _context.Projects
                .Include(p => p.Budgets)
                .Include(p => p.Deliverables)
                .Where(p => p.Status == ProjectStatus.Completed)
                .ToListAsync();

            if (!completedProjects.Any()) return 0;

            var satisfactionScores = completedProjects.Select(p =>
            {
                var budgetPerformance = p.Budgets.Any() && p.Budgets.Sum(b => b.AllocatedAmount) > 0
                    ? Math.Min(100, (p.Budgets.Sum(b => b.AllocatedAmount) - p.Budgets.Sum(b => b.SpentAmount)) / p.Budgets.Sum(b => b.AllocatedAmount) * 100 + 100)
                    : 100;

                var schedulePerformance = p.ActualEndDate.HasValue
                    ? Math.Min(100, (p.EndDate - p.ActualEndDate.Value).Days + 100)
                    : 100;

                return (budgetPerformance + schedulePerformance) / 2;
            });

            return (decimal)satisfactionScores.Average();
        }

        private async Task<List<MonthlyFinancialDto>> GetMonthlyFinancialBreakdownAsync(DateTime fromDate, DateTime toDate)
        {
            var breakdown = new List<MonthlyFinancialDto>();
            var current = new DateTime(fromDate.Year, fromDate.Month, 1);
            var end = new DateTime(toDate.Year, toDate.Month, 1);

            while (current <= end)
            {
                var monthEnd = current.AddMonths(1).AddDays(-1);

                var revenue = await _context.Invoices
                    .Where(i => i.InvoiceDate >= current && i.InvoiceDate <= monthEnd && i.Status == InvoiceStatus.Paid)
                    .SumAsync(i => i.TotalAmount);

                var expenses = await _context.VendorInvoices
                    .Where(vi => vi.InvoiceDate >= current && vi.InvoiceDate <= monthEnd && vi.Status == VendorInvoiceStatus.Paid)
                    .SumAsync(vi => vi.TotalAmount);

                breakdown.Add(new MonthlyFinancialDto
                {
                    Month = current.ToString("yyyy-MM"),
                    Revenue = revenue,
                    Expenses = expenses,
                    Profit = revenue - expenses
                });

                current = current.AddMonths(1);
            }

            return breakdown;
        }

        private string GetStatusColor(ProjectStatus status)
        {
            return status switch
            {
                ProjectStatus.Active => "#28a745",
                ProjectStatus.Completed => "#007bff",
                ProjectStatus.OnHold => "#ffc107",
                ProjectStatus.Cancelled => "#dc3545",
                _ => "#6c757d"
            };
        }

        private string GenerateRandomColor()
        {
            var random = new Random();
            return $"#{random.Next(0x1000000):X6}";
        }
    }
}

