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

            // Validation 3: Employee allocation doesn't exceed 100% across projects
            var endDate = command.EndDate ?? project.EndDate;
            var employeeAllocations = await _context.ActualAssignments
                .Where(aa => aa.EmployeeId == command.EmployeeId &&
                            !aa.IsDeleted &&
                            (aa.Status == AssignmentStatus.Active || aa.Status == AssignmentStatus.Planned) &&
                            aa.StartDate <= endDate &&
                            (aa.EndDate == null || aa.EndDate >= command.StartDate))
                .SumAsync(aa => aa.AllocationPercent);

            var totalEmployeeAllocation = employeeAllocations + command.AllocationPercent;
            if (totalEmployeeAllocation > 100)
            {
                result.Blockers.Add($"Employee total allocation ({totalEmployeeAllocation}%) would exceed 100%");
            }
            else if (totalEmployeeAllocation > 80)
            {
                result.Warnings.Add($"Employee will have high utilization ({totalEmployeeAllocation}%) during assignment period");
                result.HasUtilizationWarning = true;
            }

            // Validation 4: Check for overlapping assignments
            var overlappingAssignments = await _context.ActualAssignments
                .Where(aa => aa.EmployeeId == command.EmployeeId &&
                            !aa.IsDeleted &&
                            (aa.Status == AssignmentStatus.Active || aa.Status == AssignmentStatus.Planned) &&
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
                result.Blockers.Add($"Employee role ({employeeRole}) does not match planned slot role");
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
                result.HasCostVariance = true;
            }

            // Determine if approval is required
            result.RequiresApproval = result.HasRoleMismatch || result.HasCostVariance || Math.Abs(costDifferencePercent) > 20;

            return result;
        }
    }
}