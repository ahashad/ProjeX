using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Enums;
using ProjeX.Infrastructure.Data;
using ProjeX.Application.ActualAssignment;

namespace ProjeX.Application.ResourceUtilization
{
    public class UtilizationService : IUtilizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UtilizationService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<UtilizationSummaryDto> GetEmployeeUtilizationAsync(Guid employeeId, DateTime startDate, DateTime endDate)
        {
            var assignments = await _context.ActualAssignments
                .Include(a => a.Project)
                .Where(a => a.EmployeeId == employeeId &&
                           a.Status == AssignmentStatus.Active &&
                           a.StartDate <= endDate &&
                           (a.EndDate == null || a.EndDate >= startDate))
                .ToListAsync();

            var totalAllocation = assignments.Sum(a => a.AllocationPercent);
            var projectCount = assignments.Select(a => a.ProjectId).Distinct().Count();

            return new UtilizationSummaryDto
            {
                EmployeeId = employeeId,
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TotalAllocationPercent = totalAllocation,
                ProjectCount = projectCount,
                IsOverAllocated = totalAllocation > 100,
                IsUnderUtilized = totalAllocation < 80,
                Assignments = _mapper.Map<List<ActualAssignmentDto>>(assignments)
            };
        }

        public async Task<List<UtilizationSummaryDto>> GetTeamUtilizationAsync(DateTime startDate, DateTime endDate)
        {
            var employees = await _context.Employees
                .Where(e => e.IsActive)
                .ToListAsync();

            var utilizationSummaries = new List<UtilizationSummaryDto>();

            foreach (var employee in employees)
            {
                var utilization = await GetEmployeeUtilizationAsync(employee.Id, startDate, endDate);
                utilizationSummaries.Add(utilization);
            }

            return utilizationSummaries.OrderByDescending(u => u.TotalAllocationPercent).ToList();
        }

        public async Task<ProjectUtilizationDto> GetProjectUtilizationAsync(Guid projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ActualAssignments)
                    .ThenInclude(a => a.Employee)
                        .ThenInclude(e => e.Role)
                .Include(p => p.PlannedTeamSlots)
                    .ThenInclude(pts => pts.Role)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new ArgumentException("Project not found");

            var plannedCapacity = project.PlannedTeamSlots.Sum(pts => pts.AllocationPercent);
            var actualCapacity = project.ActualAssignments
                .Where(a => a.Status == AssignmentStatus.Active)
                .Sum(a => a.AllocationPercent);

            var utilizationByRole = project.ActualAssignments
                .Where(a => a.Status == AssignmentStatus.Active && a.Employee != null)
                .GroupBy(a => a.Employee.Role?.Name ?? "Unknown")
                .Select(g => new RoleUtilizationDto
                {
                    RoleName = g.Key,
                    TotalAllocation = g.Sum(a => a.AllocationPercent),
                    EmployeeCount = g.Select(a => a.EmployeeId).Distinct().Count()
                })
                .ToList();

            return new ProjectUtilizationDto
            {
                ProjectId = projectId,
                ProjectName = project.ProjectName,
                PlannedCapacity = plannedCapacity,
                ActualCapacity = actualCapacity,
                UtilizationPercentage = plannedCapacity > 0 ? (actualCapacity / plannedCapacity) * 100 : 0,
                RoleUtilization = utilizationByRole
            };
        }

        public async Task<List<CapacityForecastDto>> GetCapacityForecastAsync(DateTime startDate, DateTime endDate, TimeBucket timeBucket)
        {
            var forecasts = new List<CapacityForecastDto>();
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                var periodEnd = GetPeriodEnd(currentDate, timeBucket);

                // Ensure period end doesn't exceed the requested end date
                if (periodEnd > endDate)
                {
                    periodEnd = endDate;
                }

                var totalDemand = await _context.ActualAssignments
                    .Where(a => a.Status == AssignmentStatus.Active &&
                               a.StartDate <= periodEnd &&
                               (a.EndDate == null || a.EndDate >= currentDate))
                    .SumAsync(a => a.AllocationPercent);

                var totalCapacity = await _context.Employees
                    .Where(e => e.IsActive)
                    .CountAsync() * 100; // 100% per employee

                forecasts.Add(new CapacityForecastDto
                {
                    PeriodStart = currentDate,
                    PeriodEnd = periodEnd,
                    TotalCapacity = totalCapacity,
                    TotalDemand = totalDemand,
                    AvailableCapacity = totalCapacity - totalDemand,
                    UtilizationPercentage = totalCapacity > 0 ? (totalDemand / totalCapacity) * 100 : 0
                });

                currentDate = GetNextPeriod(currentDate, timeBucket);
            }

            return forecasts;
        }

        public async Task<List<ResourceRecommendationDto>> GetResourceRecommendationsAsync()
        {
            var recommendations = new List<ResourceRecommendationDto>();
            var currentDate = DateTime.UtcNow;
            var futureDate = currentDate.AddMonths(3);

            // Find over-allocated employees
            var overAllocatedEmployees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    Employee = e,
                    TotalAllocation = e.ActualAssignments
                        .Where(a => a.Status == AssignmentStatus.Active &&
                                   a.StartDate <= futureDate &&
                                   (a.EndDate == null || a.EndDate >= currentDate))
                        .Sum(a => a.AllocationPercent)
                })
                .Where(x => x.TotalAllocation > 100)
                .ToListAsync();

            foreach (var item in overAllocatedEmployees)
            {
                recommendations.Add(new ResourceRecommendationDto
                {
                    Type = "OverAllocation",
                    Priority = "High",
                    EmployeeId = item.Employee.Id,
                    EmployeeName = $"{item.Employee.FirstName} {item.Employee.LastName}",
                    Description = $"Employee is over-allocated at {item.TotalAllocation}%",
                    Recommendation = "Consider redistributing workload or hiring additional resources"
                });
            }

            // Find under-utilized employees
            var underUtilizedEmployees = await _context.Employees
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    Employee = e,
                    TotalAllocation = e.ActualAssignments
                        .Where(a => a.Status == AssignmentStatus.Active &&
                                   a.StartDate <= futureDate &&
                                   (a.EndDate == null || a.EndDate >= currentDate))
                        .Sum(a => a.AllocationPercent)
                })
                .Where(x => x.TotalAllocation < 60)
                .ToListAsync();

            foreach (var item in underUtilizedEmployees)
            {
                recommendations.Add(new ResourceRecommendationDto
                {
                    Type = "UnderUtilization",
                    Priority = "Medium",
                    EmployeeId = item.Employee.Id,
                    EmployeeName = $"{item.Employee.FirstName} {item.Employee.LastName}",
                    Description = $"Employee is under-utilized at {item.TotalAllocation}%",
                    Recommendation = "Consider assigning additional projects or training opportunities"
                });
            }

            return recommendations.OrderBy(r => r.Priority == "High" ? 1 : 2).ToList();
        }

        private DateTime GetPeriodEnd(DateTime startDate, TimeBucket timeBucket)
        {
            return timeBucket switch
            {
                TimeBucket.Weekly => startDate.AddDays(6),
                TimeBucket.Monthly => startDate.AddMonths(1).AddDays(-1),
                TimeBucket.Quarterly => startDate.AddMonths(3).AddDays(-1),
                _ => startDate.AddDays(6)
            };
        }

        private DateTime GetNextPeriod(DateTime currentDate, TimeBucket timeBucket)
        {
            return timeBucket switch
            {
                TimeBucket.Weekly => currentDate.AddDays(7),
                TimeBucket.Monthly => currentDate.AddMonths(1),
                TimeBucket.Quarterly => currentDate.AddMonths(3),
                _ => currentDate.AddDays(7)
            };
        }
    }
}

