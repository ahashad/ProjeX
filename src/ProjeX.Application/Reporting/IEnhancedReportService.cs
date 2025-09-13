namespace ProjeX.Application.Reporting
{
    public interface IEnhancedReportService
{
        Task<ExecutiveDashboardDto> GetExecutiveDashboardAsync();
 Task<ProjectPerformanceReportDto> GetProjectPerformanceReportAsync(DateTime fromDate, DateTime toDate);
    Task<ResourceUtilizationReportDto> GetResourceUtilizationReportAsync(DateTime fromDate, DateTime toDate);
        Task<FinancialReportDto> GetFinancialReportAsync(DateTime fromDate, DateTime toDate);
      Task<List<ChartDataDto>> GetProjectStatusChartDataAsync();
  Task<List<ChartDataDto>> GetRevenueByClientChartDataAsync(int topN = 10);
    Task<List<TrendDataDto>> GetUtilizationTrendAsync(int months = 12);
    }
}