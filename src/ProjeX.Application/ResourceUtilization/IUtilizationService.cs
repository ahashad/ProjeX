using ProjeX.Domain.Enums;

namespace ProjeX.Application.ResourceUtilization
{
    public interface IUtilizationService
    {
        Task<UtilizationSummaryDto> GetEmployeeUtilizationAsync(Guid employeeId, DateTime startDate, DateTime endDate);
        Task<List<UtilizationSummaryDto>> GetTeamUtilizationAsync(DateTime startDate, DateTime endDate);
        Task<ProjectUtilizationDto> GetProjectUtilizationAsync(Guid projectId);
        Task<List<CapacityForecastDto>> GetCapacityForecastAsync(DateTime startDate, DateTime endDate, TimeBucket timeBucket);
        Task<List<ResourceRecommendationDto>> GetResourceRecommendationsAsync();
    }
}

