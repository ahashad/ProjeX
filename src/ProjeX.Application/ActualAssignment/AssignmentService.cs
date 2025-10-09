using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Application.ActualAssignment.Commands;
using ProjeX.Application.Employee;
using ProjeX.Domain.Entities;
using ProjeX.Domain.Enums;
using ProjeX.Infrastructure.Data;

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

        public async System.Threading.Tasks.Task<AssignmentCreationResult> CreateAsync(CreateActualAssignmentCommand command, string userId)
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
                RequiresApproval = preCheckResult.RequiresApproval,

                // Salary snapshot: default from Employee or PlannedTeamSlot if not provided in command
                SnapshotSalary = command.SnapshotSalary ?? employee.Salary,
                SnapshotMonthlyIncentive = command.SnapshotMonthlyIncentive ?? employee.MonthlyIncentive,
                SnapshotCommissionPercent = command.SnapshotCommissionPercent ?? employee.CommissionPercent,
                SnapshotTickets = command.SnapshotTickets ?? plannedTeamSlot.PlannedTickets,
                SnapshotHoteling = command.SnapshotHoteling ?? plannedTeamSlot.PlannedHoteling,
                SnapshotOthers = command.SnapshotOthers ?? plannedTeamSlot.PlannedOthers
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

        public async System.Threading.Tasks.Task ApproveAsync(Guid assignmentId, string approverUserId)
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

            // Update planned team slot status to Active
            var plannedTeamSlot = await _context.PlannedTeamSlots.FindAsync(assignment.PlannedTeamSlotId);
            if (plannedTeamSlot == null)
                throw new ArgumentException("Planned team slot not found");
            plannedTeamSlot.Status = PlannedTeamStatus.Active;

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task RejectAsync(Guid assignmentId, string approverUserId, string reason)
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

        public async System.Threading.Tasks.Task DeclineAsync(Guid assignmentId, string approverUserId, string reason)
        {
            var assignment = await _context.ActualAssignments.FindAsync(assignmentId);
            if (assignment == null)
            {
                throw new ArgumentException("Assignment not found");
            }

            // Validate that assignment is in Planned status
            if (assignment.Status != AssignmentStatus.Planned)
            {
                throw new InvalidOperationException($"Cannot decline assignment with status {assignment.Status}. Only Planned assignments can be declined.");
            }

            // Transition to Cancelled status
            assignment.Status = AssignmentStatus.Cancelled;
            assignment.ApprovedByUserId = approverUserId;
            assignment.ApprovedOn = DateTime.UtcNow;
            assignment.Notes = $"{assignment.Notes}\n\nDECLINED: {reason}".Trim();
            assignment.ModifiedBy = approverUserId;
            assignment.ModifiedAt = DateTime.UtcNow;

            // Note: PlannedSlot status remains independent and unchanged
            // The slot is now freed up for reassignment since this assignment is cancelled

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task<AssignmentUpdateResult> UpdateAsync(UpdateActualAssignmentCommand command, string userId)
        {
            var result = new AssignmentUpdateResult();

            try
            {
                var assignment = await _context.ActualAssignments
                    .Include(a => a.Employee)
                    .Include(a => a.Project)
                    .Include(a => a.PlannedTeamSlot)
                    .FirstOrDefaultAsync(a => a.Id == command.Id && !a.IsDeleted);

                if (assignment == null)
                {
                    result.IsSuccessful = false;
                    result.Errors.Add("Assignment not found");
                    return result;
                }

                //// Validation: EndDate must not be in the future
                //if (command.EndDate.HasValue && command.EndDate.Value.Date > DateTime.Today)
                //{
                //    result.IsSuccessful = false;
                //    result.Errors.Add("End date cannot be in the future");
                //    return result;
                //}

                // Validation: EndDate must not be before StartDate
                if (command.EndDate.HasValue && command.EndDate.Value.Date < command.StartDate.Date)
                {
                    result.IsSuccessful = false;
                    result.Errors.Add("End date cannot be before start date");
                    return result;
                }

                // Validation: StartDate must be within project date range
                if (command.StartDate < assignment.Project.StartDate)
                {
                    result.IsSuccessful = false;
                    result.Errors.Add("Start date cannot be before project start date");
                    return result;
                }

                if (command.StartDate > assignment.Project.EndDate)
                {
                    result.IsSuccessful = false;
                    result.Errors.Add("Start date must be within project date range");
                    return result;
                }

                if (command.EndDate.HasValue && command.EndDate.Value > assignment.Project.EndDate)
                {
                    result.IsSuccessful = false;
                    result.Errors.Add("End date cannot be after project end date");
                    return result;
                }

                // Validation: Allocation percent
                if (command.AllocationPercent <= 0 || command.AllocationPercent > 100)
                {
                    result.IsSuccessful = false;
                    result.Errors.Add("Allocation percent must be between 0 and 100");
                    return result;
                }

                // Update assignment fields
                assignment.StartDate = command.StartDate;
                assignment.EndDate = command.EndDate;
                assignment.AllocationPercent = command.AllocationPercent;
                assignment.Notes = command.Notes;
                assignment.SnapshotSalary = command.SnapshotSalary;
                assignment.SnapshotMonthlyIncentive = command.SnapshotMonthlyIncentive;
                assignment.SnapshotCommissionPercent = command.SnapshotCommissionPercent;
                assignment.SnapshotTickets = command.SnapshotTickets;
                assignment.SnapshotHoteling = command.SnapshotHoteling;
                assignment.SnapshotOthers = command.SnapshotOthers;
                assignment.ModifiedBy = userId;
                assignment.ModifiedAt = DateTime.UtcNow;

                // Auto-complete if EndDate is set and in the past
                if (assignment.EndDate.HasValue && assignment.EndDate.Value.Date < DateTime.Today)
                {
                    assignment.Status = AssignmentStatus.Completed;
                    result.Warnings.Add("Assignment has been automatically completed because end date is in the past");
                }

                await _context.SaveChangesAsync();

                // Reload with full includes for DTO mapping
                var updatedAssignment = await GetByIdAsync(assignment.Id);
                result.Assignment = updatedAssignment;
                result.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.Errors.Add($"Error updating assignment: {ex.Message}");
            }

            return result;
        }

        public async System.Threading.Tasks.Task DeleteAsync(Guid assignmentId, string userId)
        {
            var assignment = await _context.ActualAssignments
                .Include(a => a.PlannedTeamSlot)
                .FirstOrDefaultAsync(a => a.Id == assignmentId && !a.IsDeleted);

            if (assignment == null)
            {
                throw new ArgumentException("Assignment not found");
            }

            // Soft delete
            assignment.IsDeleted = true;
            assignment.ModifiedBy = userId;
            assignment.ModifiedAt = DateTime.UtcNow;

            // Update planned team slot status if no other active/planned assignments
            if (assignment.PlannedTeamSlotId.HasValue)
            {
                var hasOtherAssignments = await _context.ActualAssignments
                    .AnyAsync(a => a.PlannedTeamSlotId == assignment.PlannedTeamSlotId &&
                                  a.Id != assignmentId &&
                                  !a.IsDeleted &&
                                  (a.Status == AssignmentStatus.Active || a.Status == AssignmentStatus.Planned));

                if (!hasOtherAssignments && assignment.PlannedTeamSlot != null)
                {
                    assignment.PlannedTeamSlot.Status = PlannedTeamStatus.Planned;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UnassignAsync(UnassignActualAssignmentCommand command, string userId)
        {
            var assignment = await _context.ActualAssignments.FindAsync(command.AssignmentId);
            if (assignment == null)
            {
                throw new ArgumentException("Assignment not found");
            }

            // Validation: EndDate must not be in the future
            if (command.EndDate.Date > DateTime.Today)
            {
                throw new ArgumentException("End date cannot be in the future");
            }

            // Validation: EndDate must not be before StartDate
            if (command.EndDate.Date < assignment.StartDate.Date)
            {
                throw new ArgumentException("End date cannot be before start date");
            }

            // Update planned team slot status to Planned
            var plannedTeamSlot = await _context.PlannedTeamSlots.FindAsync(assignment.PlannedTeamSlotId);
            if (plannedTeamSlot == null)
                throw new ArgumentException("Planned team slot not found");
            plannedTeamSlot.Status = PlannedTeamStatus.Planned;

            assignment.EndDate = command.EndDate;
            assignment.Status = AssignmentStatus.Completed;
            assignment.ModifiedBy = userId;
            assignment.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task<List<ActualAssignmentDto>> GetAssignmentsAsync(Guid? projectId, Guid? employeeId)
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

        public async System.Threading.Tasks.Task<ActualAssignmentDto?> GetByIdAsync(Guid id)
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

        public async System.Threading.Tasks.Task<decimal> GetEmployeeAllocationAsync(Guid employeeId, DateTime startDate, DateTime endDate)
        {
            return await _context.ActualAssignments
                .Where(aa => aa.EmployeeId == employeeId &&
                    !aa.IsDeleted &&
                    (aa.Status == AssignmentStatus.Planned || aa.Status == AssignmentStatus.Active) &&
                    aa.StartDate <= endDate &&
                    (aa.EndDate == null || aa.EndDate >= startDate))
                .SumAsync(aa => aa.AllocationPercent);
        }

        public async System.Threading.Tasks.Task<List<EmployeeUtilizationPointDto>> GetEmployeeUtilizationAsync(Guid employeeId, DateTime from, DateTime to)
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

        public async System.Threading.Tasks.Task<AssignmentCreationResult> ValidateAssignmentAsync(CreateActualAssignmentCommand command)
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

        private async System.Threading.Tasks.Task<AssignmentPreCheckResult> PerformPreAssignmentChecksAsync(
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


            ////comment this code block to Allow to allocate more than 100% to planned slot.
            //// Validation 2: Slot allocation doesn't exceed AllowedAllocation%
            //var existingSlotAllocations = await _context.ActualAssignments
            //    .Where(aa => aa.PlannedTeamSlotId == command.PlannedTeamSlotId &&
            //                !aa.IsDeleted &&
            //                (aa.Status == AssignmentStatus.Active || aa.Status == AssignmentStatus.Planned))
            //    .SumAsync(aa => aa.AllocationPercent);

            //var totalSlotAllocation = existingSlotAllocations + command.AllocationPercent;
            //if (totalSlotAllocation > plannedTeamSlot.AllocationPercent)
            //{
            //    result.Blockers.Add($"Assignment allocation ({command.AllocationPercent}%) exceeds planned slot allocation ({plannedTeamSlot.AllocationPercent}%)");
            //}
            ////comment this code block to Allow to allocate more than 100% to planned slot.


            // Validation 3: Timeline-based employee allocation validation
            var timelineValidation = await ValidateEmployeeTimelineAllocationAsync(command);

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
                            aa.StartDate <= command.EndDate &&
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
            result.RequiresApproval = result is { HasRoleMismatch: true, HasCostVariance: true };

            return result;
        }

        /// <summary>
        /// ValidaCreateActualAssignmentCommandtes Planned slot allocation across timeline to ensure no point in time exceeds 100%
        /// </summary>
        private async System.Threading.Tasks.Task<TimelineValidationResult> ValidateEmployeeTimelineAllocationAsync(
            CreateActualAssignmentCommand command)
        {
            var result = new TimelineValidationResult();

            // Get all existing assignments for this employee that overlap with the new assignment
            var existingAssignments = await _context.ActualAssignments
                .Where(aa =>
                    aa.EmployeeId == command.EmployeeId &&
                    !aa.IsDeleted &&
                    aa.Status != AssignmentStatus.Cancelled &&
                    aa.EndDate >= command.StartDate &&
                    aa.StartDate <= command.EndDate)
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
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                AllocationPercent = command.AllocationPercent,
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
                    EndDate = command.EndDate ?? DateTime.MaxValue.Date,
                    MaxAllocation = currentAllocation
                });
            }

            result.Violations = violations;
            result.HasViolations = violations.Any();

            return result;
        }

        public async System.Threading.Tasks.Task<List<ActualAssignmentDto>> GetAssignmentsBySlotAsync(Guid plannedTeamSlotId, Guid roleId)
        {
            var slot = await _context.PlannedTeamSlots
                .AsNoTracking()
                .Include(pts => pts.Role)
                .FirstOrDefaultAsync(pts => pts.Id == plannedTeamSlotId);
            if (slot == null)
            {
                // Slot not found
                return new List<ActualAssignmentDto>();
            }

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
                       a.Status != AssignmentStatus.Cancelled)
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

                // Get cost information - prioritize snapshot, fallback to employee
                var salary = assignment.SnapshotSalary ?? assignment.Employee?.Salary ?? 0;
                var monthlyIncentive = assignment.SnapshotMonthlyIncentive ?? assignment.Employee?.MonthlyIncentive ?? 0;
                var commissionPercent = assignment.SnapshotCommissionPercent ?? assignment.Employee?.CommissionPercent ?? 0;
                var tickets = assignment.SnapshotTickets ?? 0;
                var hoteling = assignment.SnapshotHoteling ?? 0;
                var others = assignment.SnapshotOthers ?? 0;

                dto.EmployeeSalary = salary;
                dto.EmployeeMonthlyIncentive = monthlyIncentive;
                dto.EmployeeCommissionPercent = commissionPercent;

                // Store snapshot values in DTO
                dto.SnapshotSalary = assignment.SnapshotSalary;
                dto.SnapshotMonthlyIncentive = assignment.SnapshotMonthlyIncentive;
                dto.SnapshotCommissionPercent = assignment.SnapshotCommissionPercent;
                dto.SnapshotTickets = assignment.SnapshotTickets;
                dto.SnapshotHoteling = assignment.SnapshotHoteling;
                dto.SnapshotOthers = assignment.SnapshotOthers;

                // NetProjectPrice = ProjectPrice after deducting 15% VAT
                var netProjectPrice = assignment.Project.ProjectPrice / 1.15m;

                // Calculate actual cost using snapshot values
                var monthsWorked = dto.DurationDays / 30.0m;
                var commissionAmount = (commissionPercent / 100m) * netProjectPrice * (dto.UtilizationPercent / 100); // One-time commission
                dto.ActualCost = ((salary + monthlyIncentive + tickets + hoteling + others) * monthsWorked) + commissionAmount;


                // This represents the planned cost for THIS assignment based on planned slot values
                // Note: This is NOT the total planned cost of the entire slot if it had been but depends on utilization %
                dto.PlannedCostShare = ((slot.PlannedSalary + slot.PlannedIncentive + slot.PlannedTickets + slot.PlannedHoteling + slot.PlannedOthers) * monthsWorked) + ((slot.PlannedCommissionPercent / 100m) * netProjectPrice * (dto.UtilizationPercent / 100)); ;

                // Calculate cost variance (positive = over budget, negative = under budget)
                // Note: With Option C (using snapshot values for both), variance should be 0 unless snapshot values differ
                dto.CostVariance = dto.ActualCost - dto.PlannedCostShare;

                // Set timeline boundaries
                dto.TimelineStart = assignment.StartDate;
                dto.TimelineEnd = assignment.EndDate ?? DateTime.Today;

                dtos.Add(dto);
            }

            return dtos;
        }

        public async System.Threading.Tasks.Task<AutoCompleteResult> AutoCompleteAssignmentsAsync(string userId)
        {
            var startTime = DateTime.UtcNow;
            var result = new AutoCompleteResult();

            try
            {
                var today = DateTime.Today;

                // Find all Active assignments where EndDate < today (past due)
                var dueAssignments = await _context.ActualAssignments
                    .Where(a => !a.IsDeleted &&
                                a.Status == AssignmentStatus.Active &&
                                a.EndDate.HasValue &&
                                a.EndDate.Value < today)
                    .ToListAsync();

                foreach (var assignment in dueAssignments)
                {
                    // Update status to Completed
                    assignment.Status = AssignmentStatus.Completed;
                    assignment.ModifiedBy = userId;
                    assignment.ModifiedAt = DateTime.UtcNow;

                    // Add to completed list for telemetry
                    result.CompletedAssignmentIds.Add(assignment.Id);
                }

                // Save changes atomically
                if (dueAssignments.Any())
                {
                    await _context.SaveChangesAsync();
                }

                result.CompletedCount = dueAssignments.Count;
                result.IsSuccessful = true;
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.Errors.Add($"Error auto-completing assignments: {ex.Message}");
            }
            finally
            {
                result.Duration = DateTime.UtcNow - startTime;
            }

            return result;
        }

        public async System.Threading.Tasks.Task<DateTime?> GetMostRecentCompletedAssignmentEndDateAsync(Guid projectId,
            Guid plannedTeamSlotId)
        {
            // Get the most recent completed assignment's end date for any employee in this project
            var mostRecentEndDate = await _context.ActualAssignments
                .AsNoTracking()
                .Where(a => a.ProjectId == projectId &&
                            a.PlannedTeamSlotId == plannedTeamSlotId &&
                           !a.IsDeleted &&
                           a.Status == AssignmentStatus.Completed &&
                           a.EndDate.HasValue)
                .OrderByDescending(a => a.EndDate)
                .Select(a => a.EndDate)
                .FirstOrDefaultAsync();

            return mostRecentEndDate;
        }

        public async System.Threading.Tasks.Task<AssignmentValidationResult> ValidateAssignmentRealtimeAsync(
            CreateActualAssignmentCommand command)
        {
            var result = new AssignmentValidationResult
            {
                IsValid = true,
                Severity = ValidationSeverity.None
            };

            // Basic validation
            if (command.EmployeeId == Guid.Empty)
            {
                result.IsValid = false;
                result.Severity = ValidationSeverity.Error;
                result.BlockingErrors.Add("Employee must be selected");
                return result;
            }

            if (command.AllocationPercent <= 0 || command.AllocationPercent > 100)
            {
                result.IsValid = false;
                result.Severity = ValidationSeverity.Error;
                result.BlockingErrors.Add("Allocation percent must be between 0 and 100");
                return result;
            }

            if (command.EndDate.HasValue && command.StartDate > command.EndDate.Value)
            {
                result.IsValid = false;
                result.Severity = ValidationSeverity.Error;
                result.BlockingErrors.Add("Start date must be before or equal to end date");
                return result;
            }

            // Validate entities exist
            var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == command.ProjectId);
            if (project == null)
            {
                result.IsValid = false;
                result.Severity = ValidationSeverity.Error;
                result.BlockingErrors.Add("Project not found");
                return result;
            }

            var employee = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == command.EmployeeId);
            if (employee == null)
            {
                result.IsValid = false;
                result.Severity = ValidationSeverity.Error;
                result.BlockingErrors.Add("Employee not found");
                return result;
            }

            var plannedSlot = await _context.PlannedTeamSlots
                .AsNoTracking()
                .Include(pts => pts.Role)
                .FirstOrDefaultAsync(pts => pts.Id == command.PlannedTeamSlotId);
            if (plannedSlot == null)
            {
                result.IsValid = false;
                result.Severity = ValidationSeverity.Error;
                result.BlockingErrors.Add("Planned team slot not found");
                return result;
            }

            // Validate dates within project range
            if (command.StartDate < project.StartDate)
            {
                result.IsValid = false;
                result.Severity = ValidationSeverity.Error;
                result.BlockingErrors.Add($"Start date cannot be before project start date ({project.StartDate:yyyy-MM-dd})");
            }

            if (command.StartDate > project.EndDate)
            {
                result.IsValid = false;
                result.Severity = ValidationSeverity.Error;
                result.BlockingErrors.Add($"Start date must be within project date range (ends {project.EndDate:yyyy-MM-dd})");
            }

            if (command.EndDate.HasValue && command.EndDate.Value > project.EndDate)
            {
                result.IsValid = false;
                result.Severity = ValidationSeverity.Error;
                result.BlockingErrors.Add($"End date cannot be after project end date ({project.EndDate:yyyy-MM-dd})");
            }

            // If basic validations failed, return early
            if (!result.IsValid)
            {
                return result;
            }

            // Get conflicting assignments with full details
            var effectiveEndDate = command.EndDate ?? project.EndDate ?? command.StartDate.AddYears(1);
            var conflictingAssignments = await _context.ActualAssignments
                .AsNoTracking()
                .Include(a => a.Project)
                .Include(a => a.PlannedTeamSlot)
                    .ThenInclude(pts => pts.Role)
                .Where(a => a.EmployeeId == command.EmployeeId &&
                           !a.IsDeleted &&
                           a.Status != AssignmentStatus.Cancelled &&
                           a.StartDate <= effectiveEndDate &&
                           (a.EndDate == null || a.EndDate >= command.StartDate))
                .Select(a => new ConflictingAssignmentDetail
                {
                    AssignmentId = a.Id,
                    ProjectName = a.Project.ProjectName,
                    RoleName = a.PlannedTeamSlot != null ? a.PlannedTeamSlot.Role.RoleName : "Unknown",
                    StartDate = a.StartDate,
                    EndDate = a.EndDate,
                    AllocationPercent = a.AllocationPercent,
                    Status = a.Status.ToString(),
                    EmployeeName = employee.FullName
                })
                .ToListAsync();

            result.ConflictingAssignments = conflictingAssignments;
            result.HasConflicts = conflictingAssignments.Any();

            // Calculate daily allocations and check for violations
            var dailyAllocations = CalculateDailyAllocations(
                command.StartDate,
                effectiveEndDate,
                command.AllocationPercent,
                conflictingAssignments);

            result.DailyAllocations = dailyAllocations;

            if (dailyAllocations.Any())
            {
                var maxOverallocation = dailyAllocations.Max(d => d.TotalAllocationPercent);
                result.MaxAllocationFoundPercent = maxOverallocation;
                result.MaxAllocationDate = dailyAllocations
                    .FirstOrDefault(d => d.TotalAllocationPercent == maxOverallocation)?.Date;

                var minRemaining = dailyAllocations.Min(d => d.RemainingPercent);
                result.RemainingCapacityPercent = minRemaining;

                // Check for allocation violations
                var overallocatedDays = dailyAllocations.Where(d => d.IsOverallocated).ToList();
                if (overallocatedDays.Any())
                {
                    result.IsValid = false;
                    result.Severity = ValidationSeverity.Error;
                    var firstViolation = overallocatedDays.First();
                    var lastViolation = overallocatedDays.Last();
                    result.BlockingErrors.Add(
                        $"Employee allocation exceeds 100% from {firstViolation.Date:yyyy-MM-dd} to {lastViolation.Date:yyyy-MM-dd} (peak: {maxOverallocation:F1}%)");

                    // Calculate suggested windows
                    result.SuggestedWindows = CalculateSuggestedWindows(
                        command.StartDate,
                        effectiveEndDate,
                        command.AllocationPercent,
                        conflictingAssignments,
                        project.StartDate ?? command.StartDate,
                        project.EndDate ?? effectiveEndDate);
                }
                else if (maxOverallocation > 80)
                {
                    result.Severity = ValidationSeverity.Warning;
                    result.Warnings.Add($"High utilization: Employee will reach {maxOverallocation:F1}% allocation on {result.MaxAllocationDate:yyyy-MM-dd}");
                }
            }

            // Role mismatch check
            if (employee.RoleId != plannedSlot.RoleId)
            {
                if (result.Severity < ValidationSeverity.Warning)
                    result.Severity = ValidationSeverity.Warning;
                result.Warnings.Add($"Role mismatch: Employee is {employee.Role?.RoleName ?? "Unknown"} but slot requires {plannedSlot.Role?.RoleName ?? "Unknown"}");
            }

            ////comment this code block to Allow to allocate more than 100% to planned slot.
            //// Slot allocation check
            //var existingSlotAllocations = await _context.ActualAssignments
            //    .Where(aa => aa.PlannedTeamSlotId == command.PlannedTeamSlotId &&
            //                !aa.IsDeleted &&
            //                (aa.Status == AssignmentStatus.Active || aa.Status == AssignmentStatus.Planned)
            //                && aa.EndDate <= effectiveEndDate && aa.StartDate >= command.StartDate)
            //    .SumAsync(aa => aa.AllocationPercent);

            //var totalSlotAllocation = existingSlotAllocations + command.AllocationPercent;
            //if (totalSlotAllocation > plannedSlot.AllocationPercent)
            //{
            //    result.IsValid = false;
            //    result.Severity = ValidationSeverity.Error;
            //    result.BlockingErrors.Add(
            //        $"Assignment allocation ({command.AllocationPercent:F1}%) exceeds planned slot remaining capacity ({plannedSlot.AllocationPercent - existingSlotAllocations:F1}%)");
            //}
            ////comment this code block to Allow to allocate more than 100% to planned slot.

            // Overlapping assignments count
            if (conflictingAssignments.Any())
            {
                if (result.Severity < ValidationSeverity.Info)
                    result.Severity = ValidationSeverity.Info;
                result.Messages.Add($"Employee has {conflictingAssignments.Count} overlapping assignment(s) during this period");
            }

            return result;
        }

        private List<DailyAllocation> CalculateDailyAllocations(
            DateTime startDate,
            DateTime endDate,
            decimal newAllocationPercent,
            List<ConflictingAssignmentDetail> existingAssignments)
        {
            var dailyAllocations = new List<DailyAllocation>();

            for (var day = startDate.Date; day <= endDate.Date; day = day.AddDays(1))
            {
                var existingAllocation = existingAssignments
                    .Where(a => a.StartDate.Date <= day && (a.EndDate == null || a.EndDate.Value.Date >= day))
                    .Sum(a => a.AllocationPercent);

                var totalAllocation = existingAllocation + newAllocationPercent;

                dailyAllocations.Add(new DailyAllocation
                {
                    Date = day,
                    TotalAllocationPercent = totalAllocation,
                    RemainingPercent = 100 - totalAllocation
                });
            }

            return dailyAllocations;
        }

        private List<SuggestedWindow> CalculateSuggestedWindows(
            DateTime requestedStart,
            DateTime requestedEnd,
            decimal requestedAllocation,
            List<ConflictingAssignmentDetail> existingAssignments,
            DateTime projectStart,
            DateTime projectEnd)
        {
            var suggestions = new List<SuggestedWindow>();
            var requestedDuration = (requestedEnd - requestedStart).Days + 1;

            // Find all available windows in the project timeline
            var availableWindows = new List<(DateTime Start, DateTime End, decimal AvailableAllocation)>();

            for (var day = projectStart.Date; day <= projectEnd.Date; day = day.AddDays(1))
            {
                var existingAllocation = existingAssignments
                    .Where(a => a.StartDate.Date <= day && (a.EndDate == null || a.EndDate.Value.Date >= day))
                    .Sum(a => a.AllocationPercent);

                var available = 100 - existingAllocation;

                if (available >= requestedAllocation)
                {
                    // Start of a potential window
                    var windowStart = day;
                    var windowEnd = day;

                    // Extend window as far as possible
                    for (var nextDay = day.AddDays(1); nextDay <= projectEnd.Date; nextDay = nextDay.AddDays(1))
                    {
                        var nextDayExisting = existingAssignments
                            .Where(a => a.StartDate.Date <= nextDay && (a.EndDate == null || a.EndDate.Value.Date >= nextDay))
                            .Sum(a => a.AllocationPercent);

                        var nextDayAvailable = 100 - nextDayExisting;

                        if (nextDayAvailable >= requestedAllocation)
                        {
                            windowEnd = nextDay;
                        }
                        else
                        {
                            break;
                        }
                    }

                    availableWindows.Add((windowStart, windowEnd, available));
                    day = windowEnd; // Skip to end of this window
                }
            }

            // Prioritize windows
            // 1. Best fit (closest to requested dates)
            var bestFit = availableWindows
                .Where(w => (w.End - w.Start).Days + 1 >= requestedDuration)
                .OrderBy(w => Math.Abs((w.Start - requestedStart).Days))
                .FirstOrDefault();

            if (bestFit != default)
            {
                var adjustedEnd = bestFit.Start.AddDays(requestedDuration - 1);
                if (adjustedEnd > bestFit.End)
                    adjustedEnd = bestFit.End;

                suggestions.Add(new SuggestedWindow
                {
                    StartDate = bestFit.Start,
                    EndDate = adjustedEnd,
                    MaxAllocationAvailable = bestFit.AvailableAllocation,
                    DurationDays = (adjustedEnd - bestFit.Start).Days + 1,
                    Reason = "Closest to requested dates",
                    Priority = SuggestionPriority.BestFit
                });
            }

            // 2. Earliest available
            var earliest = availableWindows
                .Where(w => (w.End - w.Start).Days + 1 >= requestedDuration)
                .OrderBy(w => w.Start)
                .FirstOrDefault();

            if (earliest != default && earliest.Start != bestFit.Start)
            {
                var adjustedEnd = earliest.Start.AddDays(requestedDuration - 1);
                if (adjustedEnd > earliest.End)
                    adjustedEnd = earliest.End;

                suggestions.Add(new SuggestedWindow
                {
                    StartDate = earliest.Start,
                    EndDate = adjustedEnd,
                    MaxAllocationAvailable = earliest.AvailableAllocation,
                    DurationDays = (adjustedEnd - earliest.Start).Days + 1,
                    Reason = "Earliest available window",
                    Priority = SuggestionPriority.Earliest
                });
            }

            // 3. Longest duration
            var longest = availableWindows
                .OrderByDescending(w => (w.End - w.Start).Days)
                .FirstOrDefault();

            if (longest != default && longest.Start != bestFit.Start && longest.Start != earliest.Start)
            {
                suggestions.Add(new SuggestedWindow
                {
                    StartDate = longest.Start,
                    EndDate = longest.End,
                    MaxAllocationAvailable = longest.AvailableAllocation,
                    DurationDays = (longest.End - longest.Start).Days + 1,
                    Reason = "Longest available window",
                    Priority = SuggestionPriority.Longest
                });
            }

            return suggestions.OrderBy(s => s.Priority).Take(3).ToList();
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