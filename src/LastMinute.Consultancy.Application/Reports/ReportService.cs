using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LastMinute.Consultancy.Application.Reports.Queries;
using LastMinute.Consultancy.Infrastructure.Data;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Application.Reports
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UtilizationReportDto> GetUtilizationAsync(GetUtilizationReportQuery request)
        {
            var employees = await _context.Employees
                .Include(e => e.Role)
                .Where(e => !e.IsDeleted)
                .Where(e => !request.EmployeeId.HasValue || e.Id == request.EmployeeId.Value)
                .ToListAsync();

            var employeeUtilization = new List<EmployeeUtilizationDto>();

            foreach (var employee in employees)
            {
                var activeAssignments = await _context.ActualAssignments
                    .Include(a => a.PlannedTeamSlot)
                    .ThenInclude(pts => pts.Project)
                    .ThenInclude(p => p.Client)
                    .Include(a => a.PlannedTeamSlot)
                    .ThenInclude(pts => pts.Role)
                    .Include(a => a.Employee)
                    .Where(a => a.EmployeeId == employee.Id &&
                               !a.IsDeleted &&
                               a.Status == AssignmentStatus.Active)
                    .ToListAsync();

                var workingDays = CalculateWorkingDays(request.StartDate, request.EndDate);
                var totalAvailableHours = workingDays * 8;

                var isOnBeach = !activeAssignments.Any();
                var estimatedHoursWorked = isOnBeach ? 0 : activeAssignments.Sum(a => a.AllocationPercent / 100m * workingDays * 8);
                var utilizationRate = totalAvailableHours > 0 ? estimatedHoursWorked / totalAvailableHours * 100 : 0;

                var projectAssignments = activeAssignments.Select(a => new ProjectAssignmentDto
                {
                    ProjectId = a.PlannedTeamSlot.Project.Id,
                    ProjectName = a.PlannedTeamSlot.Project.ProjectName,
                    ClientName = a.PlannedTeamSlot.Project.Client.ClientName,
                    HoursWorked = a.AllocationPercent / 100m * workingDays * 8,
                    HourlyRate = 0
                }).ToList();

                employeeUtilization.Add(new EmployeeUtilizationDto
                {
                    EmployeeId = employee.Id,
                    EmployeeName = employee.FullName,
                    RoleName = employee.Role?.RoleName ?? "Unknown",
                    TotalHoursWorked = (decimal)estimatedHoursWorked,
                    TotalAvailableHours = totalAvailableHours,
                    UtilizationRate = (decimal)utilizationRate,
                    BillableHours = (decimal)estimatedHoursWorked,
                    NonBillableHours = 0,
                    IsOnBeach = isOnBeach,
                    ActiveAssignments = projectAssignments
                });
            }

            var overallUtilization = employeeUtilization.Any()
                ? employeeUtilization.Average(e => e.UtilizationRate)
                : 0;

            return new UtilizationReportDto
            {
                EmployeeUtilization = employeeUtilization,
                ReportStartDate = request.StartDate,
                ReportEndDate = request.EndDate,
                OverallUtilizationRate = overallUtilization
            };
        }

        private int CalculateWorkingDays(DateTime startDate, DateTime endDate)
        {
            var workingDays = 0;
            var currentDate = startDate;

            while (currentDate <= endDate)
            {
                if (currentDate.DayOfWeek != DayOfWeek.Saturday && currentDate.DayOfWeek != DayOfWeek.Sunday)
                {
                    workingDays++;
                }
                currentDate = currentDate.AddDays(1);
            }

            return workingDays;
        }
    }
}

