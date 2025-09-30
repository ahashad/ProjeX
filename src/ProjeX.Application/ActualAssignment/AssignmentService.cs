using AutoMapper;
using ProjeX.Application.Employee;
using ProjeX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Enums;
using ProjeX.Application.ActualAssignment.Commands;

namespace ProjeX.Application.ActualAssignment
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public AssignmentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<AssignmentCreationResult> CreateAsync(CreateActualAssignmentCommand command, string userId)
        {
            var result = new AssignmentCreationResult();

            // Validate project exists
            var project = await _context.Projects.FindAsync(command.ProjectId);
            if (project == null)
            {
                throw new ArgumentException("Project not found");
            }

            // Validate employee exists
            var employee = await _context.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == command.EmployeeId);
            if (employee == null)
            {
                throw new ArgumentException("Employee not found");
            }

            // Validate planned team slot exists
            var plannedTeamSlot = await _context.PlannedTeamSlots
                .Include(pts => pts.Role)
                .FirstOrDefaultAsync(pts => pts.Id == command.PlannedTeamSlotId);
            if (plannedTeamSlot == null)
            {
                throw new ArgumentException("Planned team slot not found");
            }

            // Perform pre-assignment checks according to implementation plan
            var preCheckResult = await PerformPreAssignmentChecksAsync(command, project, employee, plannedTeamSlot);
            result.Warnings.AddRange(preCheckResult.Warnings);

            if (preCheckResult.HasBlockers)
            {
                result.IsSuccessful = false;
                result.Errors.AddRange(preCheckResult.Blockers);
                return result;
            }

            // Create assignment
            var assignment = new Domain.Entities.ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = command.ProjectId,
                Project = project,
                PlannedTeamSlotId = command.PlannedTeamSlotId,
                PlannedTeamSlot = plannedTeamSlot,
                EmployeeId = command.EmployeeId,
                Employee = employee,
                AssigneeType = AssigneeType.Employee,
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                AllocationPercent = command.AllocationPercent,
                Status = AssignmentStatus.Planned,
                Notes = command.Notes,
                RequestedByUserId = userId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = userId,
                ModifiedAt = DateTime.UtcNow,

                // Set warning flags from validation
                CostCheckWarning = preCheckResult.HasCostVariance,
                CostDifferenceAmount = preCheckResult.CostVarianceAmount,
                CostDifferencePercentage = preCheckResult.CostVariancePercentage,
                UtilizationWarning = preCheckResult.HasUtilizationWarning,
                RoleMismatchWarning = preCheckResult.HasRoleMismatch,
                RequiresApproval = preCheckResult.RequiresApproval
            };

            // Establish navigation property relationships
            project.ActualAssignments.Add(assignment);
            employee.ActualAssignments.Add(assignment);

            _context.ActualAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            var assignmentDto = await GetByIdAsync(assignment.Id);
            result.Assignment = assignmentDto;
            result.IsSuccessful = true;
            return result;
        }

        public async Task ApproveAsync(Guid assignmentId, string approverUserId)
        {
            var assignment = await _context.ActualAssignments.FindAsync(assignmentId);
            if (assignment == null)
            {
                throw new ArgumentException("Assignment not found");
            }

            assignment.Status = AssignmentStatus.Active;
            assignment.ApprovedByUserId = approverUserId;
            assignment.ApprovedOn = DateTime.UtcNow;
            assignment.ModifiedBy = approverUserId;
            assignment.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task RejectAsync(Guid assignmentId, string approverUserId, string reason)
        {
            var assignment = await _context.ActualAssignments.FindAsync(assignmentId);
            if (assignment == null)
            {
                throw new ArgumentException("Assignment not found");
            }

            assignment.Status = AssignmentStatus.Cancelled;
            assignment.ApprovedByUserId = approverUserId;
            assignment.ApprovedOn = DateTime.UtcNow;
            assignment.Notes = $"{assignment.Notes}\n\nREJECTED: {reason}".Trim();
            assignment.ModifiedBy = approverUserId;
            assignment.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task UnassignAsync(UnassignActualAssignmentCommand command, string userId)
        {
            var assignment = await _context.ActualAssignments.FindAsync(command.AssignmentId);
            if (assignment == null)
            {
                throw new ArgumentException("Assignment not found");
            }

            assignment.EndDate = command.EndDate;
            assignment.Status = AssignmentStatus.Completed;
            assignment.ModifiedBy = userId;
            assignment.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<List<ActualAssignmentDto>> GetAssignmentsAsync(Guid? projectId, Guid? employeeId)
        {
            var query = _context.ActualAssignments
                .Include(a => a.Project)
                    .ThenInclude(p => p.Client)
                .Include(a => a.PlannedTeamSlot)
                    .ThenInclude(pts => pts.Role)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Role)
                .Where(a => !a.IsDeleted);

            if (projectId.HasValue)
            {
                query = query.Where(a => a.ProjectId == projectId.Value);
            }

            if (employeeId.HasValue)
            {
                query = query.Where(a => a.EmployeeId == employeeId.Value);
            }

            var assignments = await query
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<ActualAssignmentDto>>(assignments);
        }

        public async Task<ActualAssignmentDto?> GetByIdAsync(Guid id)
        {
            var assignment = await _context.ActualAssignments
                .Include(a => a.Project)
                    .ThenInclude(p => p.Client)
                .Include(a => a.PlannedTeamSlot)
                    .ThenInclude(pts => pts.Role)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Role)
                .FirstOrDefaultAsync(a => a.Id == id && !a.IsDeleted);

            return assignment != null ? _mapper.Map<ActualAssignmentDto>(assignment) : null;
        }

        public async Task<decimal> GetEmployeeAllocationAsync(Guid employeeId, DateTime startDate, DateTime endDate)
        {
            return await _context.ActualAssignments
                .Where(aa => aa.EmployeeId == employeeId &&
                    !aa.IsDeleted &&
                    (aa.Status == AssignmentStatus.Planned || aa.Status == AssignmentStatus.Active) &&
                    aa.StartDate <= endDate &&
                    (aa.EndDate == null || aa.EndDate >= startDate))
                .SumAsync(aa => aa.AllocationPercent);
        }

        public async Task<List<EmployeeUtilizationPointDto>> GetEmployeeUtilizationAsync(Guid employeeId, DateTime from, DateTime to)
        {
            var assignments = await _context.ActualAssignments
                .Where(a => a.EmployeeId == employeeId &&
                    !a.IsDeleted &&
                    (a.Status == AssignmentStatus.Planned || a.Status == AssignmentStatus.Active) &&
                    a.StartDate <= to &&
                    (a.EndDate == null || a.EndDate >= from))
                .Select(a => new { a.StartDate, a.EndDate, a.AllocationPercent })
                .ToListAsync();

            var result = new List<EmployeeUtilizationPointDto>();
            for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
            {
                var allocation = assignments
                    .Where(a => a.StartDate.Date <= day && (a.EndDate == null || a.EndDate.Value.Date >= day))
                    .Sum(a => a.AllocationPercent);

                result.Add(new EmployeeUtilizationPointDto
                {
                    Date = day,
                    AllocationPercent = allocation
                });
            }

            return result;
        }

        public async Task<AssignmentCreationResult> ValidateAssignmentAsync(CreateActualAssignmentCommand command)
        {
            var result = new AssignmentCreationResult();

            // Validate project exists
            var project = await _context.Projects.FindAsync(command.ProjectId);
            if (project == null)
            {
                result.IsSuccessful = false;
                result.Errors.Add("Project not found");
                return result;
            }

            // Validate employee exists
            var employee = await _context.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == command.EmployeeId);
            if (employee == null)
            {
                result.IsSuccessful = false;
                result.Errors.Add("Employee not found");
                return result;
            }

            // Validate planned team slot exists
            var plannedTeamSlot = await _context.PlannedTeamSlots
                .Include(pts => pts.Role)
                .FirstOrDefaultAsync(pts => pts.Id == command.PlannedTeamSlotId);
            if (plannedTeamSlot == null)
            {
                result.IsSuccessful = false;
                result.Errors.Add("Planned team slot not found");
                return result;
            }

            // Perform pre-assignment checks
            var preCheckResult = await PerformPreAssignmentChecksAsync(command, project, employee, plannedTeamSlot);
            result.Warnings.AddRange(preCheckResult.Warnings);

            if (preCheckResult.HasBlockers)
            {
                result.IsSuccessful = false;
                result.Errors.AddRange(preCheckResult.Blockers);
            }
            else
            {
                result.IsSuccessful = true;
            }

            return result;
        }

        private async Task<AssignmentPreCheckResult> PerformPreAssignmentChecksAsync(
            CreateActualAssignmentCommand command,
            Domain.Entities.Project project,
            Domain.Entities.Employee employee,
            Domain.Entities.PlannedTeamSlot plannedTeamSlot)
        {
            var result = new AssignmentPreCheckResult();

            // Validation 0: Allocation must be positive
            if (command.AllocationPercent <= 0)
            {
                result.Blockers.Add("Assignment allocation must be greater than 0%");
            }

            // Validation 1: Dates within project start/end
            if (command.StartDate < project.StartDate)
            {
                result.Blockers.Add("Assignment start date cannot be before project start date");
            }
            if (command.StartDate > project.EndDate)
            {
                result.Blockers.Add("Assignment start date must be within project date range");
            }

            if (command.EndDate.HasValue && command.EndDate.Value > project.EndDate)
            {
                result.Blockers.Add("Assignment end date cannot be after project end date");
            }

            // Validation 1b: Start date must be before end date
            if (command.EndDate.HasValue && command.StartDate > command.EndDate.Value)
            {
                result.Blockers.Add("Assignment start date must be before or equal to end date");
            }

            // Validation 2: Slot allocation doesn't exceed AllowedAllocation%
            var existingSlotAllocations = await _context.ActualAssignments
                .Where(aa => aa.PlannedTeamSlotId == command.PlannedTeamSlotId &&
                            !aa.IsDeleted &&
                            (aa.Status == AssignmentStatus.Active || aa.Status == AssignmentStatus.Planned))
                .SumAsync(aa => aa.AllocationPercent);

            var totalSlotAllocation = existingSlotAllocations + command.AllocationPercent;
            if (totalSlotAllocation > plannedTeamSlot.AllocationPercent)
            {
                result.Blockers.Add($"Assignment allocation ({command.AllocationPercent}%) exceeds planned slot allocation ({plannedTeamSlot.AllocationPercent}%)");
            }

            // Validation 3: Timeline-based employee allocation validation
            var endDate = command.EndDate ?? project.EndDate;
            var timelineValidation = await ValidateEmployeeTimelineAllocationAsync(
                command.EmployeeId,
                command.StartDate,
                endDate,
                command.AllocationPercent);

            if (timelineValidation.HasViolations)
            {
                foreach (var violation in timelineValidation.Violations)
                {
                    result.Blockers.Add($"Employee allocation would exceed 100% from {violation.StartDate:yyyy-MM-dd} to {violation.EndDate:yyyy-MM-dd} (max: {violation.MaxAllocation:F1}%)");
                }
            }
            else if (timelineValidation.MaxAllocationFound > 80)
            {
                result.Warnings.Add($"Employee will have high utilization (peak: {timelineValidation.MaxAllocationFound:F1}% on {timelineValidation.MaxAllocationDate:yyyy-MM-dd})");
                result.HasUtilizationWarning = true;
            }

            // Validation 4: Count overlapping assignments for reporting
            var overlappingAssignments = await _context.ActualAssignments
                .Where(aa => aa.EmployeeId == command.EmployeeId &&
                            !aa.IsDeleted &&
                            (aa.Status != AssignmentStatus.Cancelled) &&
                            aa.StartDate <= endDate &&
                            (aa.EndDate == null || aa.EndDate >= command.StartDate))
                .CountAsync();

            if (overlappingAssignments > 0)
            {
                result.Warnings.Add($"Employee has {overlappingAssignments} overlapping assignment(s) during this period");
            }

            // Validation 5: Role mismatch check
            if (employee.RoleId != plannedTeamSlot.RoleId)
            {
                var employeeRole = employee.Role?.RoleName ?? "Unknown";
                var plannedRole = plannedTeamSlot.Role?.RoleName ?? "Unknown";
                result.Warnings.Add($"Employee role ({employeeRole}) does not match planned slot role");
                result.HasRoleMismatch = true;
            }

            // Warning 6: Cost variance check
            var plannedMonthlyCost = plannedTeamSlot.PlannedMonthlyCost > 0 ? plannedTeamSlot.PlannedMonthlyCost : (plannedTeamSlot.PlannedSalary + plannedTeamSlot.PlannedIncentive);
            var actualMonthlyCost = (employee.Salary / 12m) + employee.MonthlyIncentive;
            var costDifference = actualMonthlyCost - plannedMonthlyCost;
            var costDifferencePercent = plannedMonthlyCost > 0 ? (costDifference / plannedMonthlyCost) * 100 : 0;

            result.CostVarianceAmount = costDifference;
            result.CostVariancePercentage = costDifferencePercent;

            if (Math.Abs(costDifferencePercent) > 10)
            {
                result.Warnings.Add($"Cost variance: {costDifferencePercent:F1}% ({(costDifference > 0 ? "over" : "under")} budget by {Math.Abs(costDifference):C})");
                result.HasCostVariance = costDifference > 0 && costDifferencePercent > 20;
            }

            // Determine if approval is required
            result.RequiresApproval = result is { HasRoleMismatch: true, HasCostVariance: true } ;

            return result;
        }

        /// <summary>
        /// Validates employee allocation across timeline to ensure no point in time exceeds 100%
        /// </summary>
        private async Task<TimelineValidationResult> ValidateEmployeeTimelineAllocationAsync(
            Guid employeeId,
            DateTime newAssignmentStart,
            DateTime? newAssignmentEnd,
            decimal newAllocationPercent,
            Guid? excludeAssignmentId = null)
        {
            var result = new TimelineValidationResult();

            // Get all existing assignments for this employee that overlap with the new assignment
            var existingAssignments = await _context.ActualAssignments
                .Where(aa => 
                    //aa.EmployeeId == employeeId &&
                    !aa.IsDeleted &&
                    aa.Status != AssignmentStatus.Cancelled &&
                    (excludeAssignmentId == null || aa.Id != excludeAssignmentId) &&
                    (newAssignmentEnd == null || aa.StartDate <= newAssignmentEnd) &&
                    (aa.EndDate == null || aa.EndDate >= newAssignmentStart))
                .Select(aa => new TimelineAssignment
                {
                    Id = aa.Id,
                    StartDate = aa.StartDate,
                    EndDate = aa.EndDate,
                    AllocationPercent = aa.AllocationPercent,
                    ProjectName = aa.Project.ProjectName
                })
                .ToListAsync();

            // Add the new assignment to the timeline
            existingAssignments.Add(new TimelineAssignment
            {
                Id = Guid.Empty,
                StartDate = newAssignmentStart,
                EndDate = newAssignmentEnd,
                AllocationPercent = newAllocationPercent,
                ProjectName = "New Assignment"
            });

            // Create timeline events (start and end points)
            var timelineEvents = new List<TimelineEvent>();

            foreach (var assignment in existingAssignments)
            {
                timelineEvents.Add(new TimelineEvent
                {
                    Date = assignment.StartDate,
                    Type = TimelineEventType.Start,
                    AllocationChange = assignment.AllocationPercent,
                    AssignmentId = assignment.Id,
                    ProjectName = assignment.ProjectName
                });

                if (assignment.EndDate.HasValue)
                {
                    // End date is inclusive, so add 1 day
                    var endEventDate = assignment.EndDate.Value.AddDays(1);
                    timelineEvents.Add(new TimelineEvent
                    {
                        Date = endEventDate,
                        Type = TimelineEventType.End,
                        AllocationChange = -assignment.AllocationPercent,
                        AssignmentId = assignment.Id,
                        ProjectName = assignment.ProjectName
                    });
                }
            }

            // Sort events by date
            timelineEvents = timelineEvents.OrderBy(e => e.Date).ThenBy(e => e.Type).ToList();

            // Process timeline to find allocation violations
            decimal currentAllocation = 0;
            DateTime? violationStart = null;
            var violations = new List<AllocationViolation>();

            foreach (var timelineEvent in timelineEvents)
            {
                currentAllocation += timelineEvent.AllocationChange;

                if (currentAllocation > 100 && violationStart == null)
                {
                    // Start of violation period
                    violationStart = timelineEvent.Date;
                }
                else if (currentAllocation <= 100 && violationStart.HasValue)
                {
                    // End of violation period
                    // Previous day was the last violation day
                    var violationEndDate = timelineEvent.Date.AddDays(-1);
                    // Before this event
                    var maxAllocationBeforeEvent = currentAllocation + timelineEvent.AllocationChange;
                    
                    violations.Add(new AllocationViolation
                    {
                        StartDate = violationStart.Value,
                        EndDate = violationEndDate,
                        MaxAllocation = maxAllocationBeforeEvent
                    });
                    violationStart = null;
                }

                // Track maximum allocation for reporting
                if (currentAllocation > result.MaxAllocationFound)
                {
                    result.MaxAllocationFound = currentAllocation;
                    result.MaxAllocationDate = timelineEvent.Date;
                }
            }

            // Handle case where violation continues to the end
            if (violationStart.HasValue)
            {
                violations.Add(new AllocationViolation
                {
                    StartDate = violationStart.Value,
                    EndDate = newAssignmentEnd ?? DateTime.MaxValue.Date,
                    MaxAllocation = currentAllocation
                });
            }

            result.Violations = violations;
            result.HasViolations = violations.Any();

            return result;
        }

        public async Task<List<ActualAssignmentDto>> GetAssignmentsBySlotAsync(Guid plannedTeamSlotId, Guid roleId)
        {
            var assignments = await _context.ActualAssignments
                .AsNoTracking()
                .Include(a => a.PlannedTeamSlot)
                    .ThenInclude(pts => pts.Project)
                .Include(a => a.PlannedTeamSlot)
                    .ThenInclude(pts => pts.Role)
                .Include(a => a.Employee)
                    .ThenInclude(e => e.Role)
                .Include(a => a.Project)
                    .ThenInclude(p => p.Client)
                .Where(a => a.PlannedTeamSlotId == plannedTeamSlotId &&
                           //a.Employee.RoleId == roleId &&
                           !a.IsDeleted &&
                           (a.Status == AssignmentStatus.Active || a.Status == AssignmentStatus.Completed))
                .OrderBy(a => a.StartDate)
                .ToListAsync();

            var dtos = new List<ActualAssignmentDto>();

            foreach (var assignment in assignments)
            {
                var dto = _mapper.Map<ActualAssignmentDto>(assignment);

                // Calculate duration in days
                var endDate = assignment.EndDate ?? DateTime.Today;
                dto.DurationDays = (endDate - assignment.StartDate).Days;

                // Calculate utilization percentage (allocation * days worked / total days available)
                var projectStartDate = assignment.Project.StartDate ?? assignment.StartDate;
                var projectEndDate = assignment.Project.EndDate ?? DateTime.Today;
                var totalDaysInProject = (projectEndDate - projectStartDate).Days;
                dto.UtilizationPercent = totalDaysInProject > 0
                    ? (assignment.AllocationPercent * dto.DurationDays) / (decimal)totalDaysInProject
                    : 0;

                // Get employee cost information
                dto.EmployeeSalary = (assignment.Employee?.Salary ?? 0);
                dto.EmployeeMonthlyIncentive = (assignment.Employee?.MonthlyIncentive ?? 0);
                dto.EmployeeCommissionPercent = (assignment.Employee?.CommissionPercent ?? 0);

                // Calculate actual cost (monthly salary + incentive + commission share)
                var monthsWorked = dto.DurationDays / 30.0m;
                var commissionAmount = ((assignment.Employee?.CommissionPercent ?? 0) / 100m) * assignment.Project.ProjectPrice;
                dto.ActualCost = ((assignment.Employee?.Salary ?? 0) + (assignment.Employee?.MonthlyIncentive ?? 0) + commissionAmount) * monthsWorked;

                // Calculate planned cost share
                var plannedCommissionAmount = ((assignment.PlannedTeamSlot?.PlannedCommissionPercent??0) / 100m) * assignment.Project.ProjectPrice;
                var plannedMonthsCost = (assignment.PlannedTeamSlot?.PeriodMonths ?? 0);
                dto.PlannedCostShare = ((assignment.PlannedTeamSlot?.PlannedSalary ?? 0) +
                                       (assignment.PlannedTeamSlot?.PlannedIncentive ?? 0) +
                                       plannedCommissionAmount) * plannedMonthsCost
                                                                * (dto.UtilizationPercent/100m);

                // Calculate cost variance (positive = over budget, negative = under budget)
                dto.CostVariance = dto.ActualCost - dto.PlannedCostShare;

                // Set timeline boundaries
                dto.TimelineStart = assignment.StartDate;
                dto.TimelineEnd = assignment.EndDate ?? DateTime.Today;

                dtos.Add(dto);
            }

            return dtos;
        }
    }

    // Helper classes for timeline validation
    public class TimelineAssignment
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal AllocationPercent { get; set; }
        public string ProjectName { get; set; } = string.Empty;
    }

    public class TimelineEvent
    {
        public DateTime Date { get; set; }
        public TimelineEventType Type { get; set; }
        public decimal AllocationChange { get; set; }
        public Guid AssignmentId { get; set; }
        public string ProjectName { get; set; } = string.Empty;
    }

    public enum TimelineEventType
    {
        Start,
        End
    }

    public class TimelineValidationResult
    {
        public bool HasViolations { get; set; }
        public List<AllocationViolation> Violations { get; set; } = new();
        public decimal MaxAllocationFound { get; set; }
        public DateTime? MaxAllocationDate { get; set; }
    }

    public class AllocationViolation
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal MaxAllocation { get; set; }
    }
}