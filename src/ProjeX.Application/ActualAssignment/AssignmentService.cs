using AutoMapper;
using ProjeX.Application.ActualAssignment.Commands;
using ProjeX.Application.Employee;
using ProjeX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Enums;

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
            
            // Perform comprehensive pre-checks
            var preCheckResult = await PerformPreChecksAsync(command);
            result.Warnings.AddRange(preCheckResult.Warnings);
            
            if (preCheckResult.HasBlockers)
            {
                result.IsSuccess = false;
                result.Errors.AddRange(preCheckResult.Blockers);
                return result;
            }

            // Create assignment with validation results
            var assignment = _mapper.Map<Domain.Entities.ActualAssignment>(command);
            assignment.Id = Guid.NewGuid();
            assignment.RequestedByUserId = userId;
            assignment.Status = AssignmentStatus.Planned;
            assignment.RequiresApproval = preCheckResult.RequiresApproval;
            
            // Set warning flags based on pre-checks
            assignment.CostCheckWarning = preCheckResult.HasCostVariance;
            assignment.UtilizationWarning = preCheckResult.HasUtilizationWarning;
            assignment.RoleMismatchWarning = preCheckResult.HasRoleMismatch;
            
            if (preCheckResult.HasCostVariance)
            {
                assignment.CostDifferenceAmount = preCheckResult.CostVarianceAmount;
                assignment.CostDifferencePercentage = preCheckResult.CostVariancePercentage;
            }

            _context.ActualAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            result.IsSuccess = true;
            result.AssignmentId = assignment.Id;
            result.Assignment = await GetByIdAsync(assignment.Id);
            
            return result;
        }

        private async Task<AssignmentPreCheckResult> PerformPreChecksAsync(CreateActualAssignmentCommand command)
        {
            var result = new AssignmentPreCheckResult();
            
            // 1. Validate basic constraints
            await ValidateBasicConstraintsAsync(command, result);
            
            // 2. Check capacity constraints
            await CheckCapacityConstraintsAsync(command, result);
            
            // 3. Check utilization constraints
            await CheckUtilizationConstraintsAsync(command, result);
            
            // 4. Check role fit
            await CheckRoleFitAsync(command, result);
            
            // 5. Check cost variance
            await CheckCostVarianceAsync(command, result);
            
            // Determine if approval is required
            result.RequiresApproval = result.HasCostVariance || result.HasUtilizationWarning || result.HasRoleMismatch;
            
            return result;
        }

        private async Task ValidateBasicConstraintsAsync(CreateActualAssignmentCommand command, AssignmentPreCheckResult result)
        {
            // Validate project exists and dates
            var project = await _context.Projects.FindAsync(command.ProjectId);
            if (project == null)
            {
                result.Blockers.Add("Project not found");
                return;
            }

            var endDate = command.EndDate ?? project.EndDate;
            
            if (command.StartDate < project.StartDate || endDate > project.EndDate)
            {
                result.Blockers.Add("Assignment dates must be within project window");
            }
            
            if (command.StartDate >= endDate)
            {
                result.Blockers.Add("Start date must be before end date");
            }

            // Validate assignee exists
            if (command.AssigneeType == AssigneeType.Employee && command.EmployeeId.HasValue)
            {
                var employee = await _context.Employees.FindAsync(command.EmployeeId.Value);
                if (employee == null)
                {
                    result.Blockers.Add("Employee not found");
                }
            }
        }

        private async Task CheckCapacityConstraintsAsync(CreateActualAssignmentCommand command, AssignmentPreCheckResult result)
        {
            if (!command.PlannedTeamSlotId.HasValue) return;

            var slot = await _context.PlannedTeamSlots.FindAsync(command.PlannedTeamSlotId.Value);
            if (slot == null) return;

            // Check if total allocation for this slot exceeds allowed allocation
            var existingAllocations = await _context.ActualAssignments
                .Where(a => a.PlannedTeamSlotId == command.PlannedTeamSlotId.Value &&
                           a.Status != AssignmentStatus.Cancelled &&
                           a.StartDate < (command.EndDate ?? DateTime.MaxValue) &&
                           (a.EndDate == null || a.EndDate > command.StartDate))
                .SumAsync(a => a.AllocationPercent);

            if (existingAllocations + command.AllocationPercent > slot.AllocationPercent)
            {
                result.Blockers.Add($"Total allocation ({existingAllocations + command.AllocationPercent}%) would exceed slot capacity ({slot.AllocationPercent}%)");
            }
        }

        private async Task CheckUtilizationConstraintsAsync(CreateActualAssignmentCommand command, AssignmentPreCheckResult result)
        {
            if (!command.EmployeeId.HasValue) return;

            // Check employee utilization across all projects
            var overlappingAssignments = await _context.ActualAssignments
                .Where(a => a.EmployeeId == command.EmployeeId.Value &&
                           a.Status == AssignmentStatus.Active &&
                           a.StartDate < (command.EndDate ?? DateTime.MaxValue) &&
                           (a.EndDate == null || a.EndDate > command.StartDate))
                .SumAsync(a => a.AllocationPercent);

            var totalUtilization = overlappingAssignments + command.AllocationPercent;
            
            if (totalUtilization > 100)
            {
                result.HasUtilizationWarning = true;
                result.Warnings.Add($"Employee utilization will be {totalUtilization}% (exceeds 100%)");
            }
        }

        private async Task CheckRoleFitAsync(CreateActualAssignmentCommand command, AssignmentPreCheckResult result)
        {
            if (!command.EmployeeId.HasValue || !command.PlannedTeamSlotId.HasValue) return;

            var employee = await _context.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == command.EmployeeId.Value);
                
            var slot = await _context.PlannedTeamSlots
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.Id == command.PlannedTeamSlotId.Value);

            if (employee?.RoleId != slot?.RoleId)
            {
                result.HasRoleMismatch = true;
                result.Warnings.Add($"Employee role ({employee?.Role?.Name}) doesn't match slot role ({slot?.Role?.Name})");
            }
        }

        private async Task CheckCostVarianceAsync(CreateActualAssignmentCommand command, AssignmentPreCheckResult result)
        {
            if (!command.EmployeeId.HasValue || !command.PlannedTeamSlotId.HasValue) return;

            var employee = await _context.Employees.FindAsync(command.EmployeeId.Value);
            var slot = await _context.PlannedTeamSlots.FindAsync(command.PlannedTeamSlotId.Value);

            if (employee == null || slot == null) return;

            var plannedMonthlyCost = slot.PlannedMonthlyCost;
            var actualMonthlyCost = employee.MonthlySalary; // Assuming this field exists

            if (plannedMonthlyCost > 0)
            {
                var variance = actualMonthlyCost - plannedMonthlyCost;
                var variancePercentage = (variance / plannedMonthlyCost) * 100;

                if (Math.Abs(variancePercentage) > 10) // 10% threshold
                {
                    result.HasCostVariance = true;
                    result.CostVarianceAmount = variance;
                    result.CostVariancePercentage = variancePercentage;
                    result.Warnings.Add($"Cost variance: {variancePercentage:F1}% ({variance:C})");
                }
            }
        }
                .ThenInclude(pts => pts.Role)
                .Include(a => a.Employee)
                .FirstOrDefaultAsync(aa => aa.PlannedTeamSlotId == command.PlannedTeamSlotId &&
                                          (aa.Status == AssignmentStatus.Planned || aa.Status == AssignmentStatus.Active) &&
                                          !aa.IsDeleted &&
                                          aa.StartDate <= endDate &&
                                          (aa.EndDate == null || aa.EndDate >= command.StartDate));

            if (overlappingAssignment != null)
            {
                var result = new AssignmentCreationResult
                {
                    ConflictingAssignment = _mapper.Map<ActualAssignmentDto>(overlappingAssignment)
                };

                var potentialEmployees = await _context.Employees
                    .Include(e => e.Role)
                    .Where(e => e.RoleId == plannedTeamSlot.RoleId && !e.IsDeleted && e.IsActive)
                    .ToListAsync();

                var available = new List<Domain.Entities.Employee>();
                foreach (var emp in potentialEmployees)
                {
                    var allocation = await GetEmployeeAllocationAsync(emp.Id, command.StartDate, endDate);
                    if (allocation + command.AllocationPercent <= 100m)
                    {
                        available.Add(emp);
                    }
                }

                result.AvailableEmployees = _mapper.Map<List<EmployeeDto>>(available);
                return result;
            }

            // Validate employee
            var employee = await _context.Employees
                .Include(e => e.Role)
                .FirstOrDefaultAsync(e => e.Id == command.EmployeeId && !e.IsDeleted);

            if (employee == null)
            {
                throw new InvalidOperationException("Employee not found");
            }

            // Check allocation limits for overlapping assignments
            var currentAllocation = await GetEmployeeAllocationAsync(employee.Id, command.StartDate, endDate);
            if (currentAllocation + command.AllocationPercent > 100m)
            {
                throw new InvalidOperationException($"Employee allocation would exceed 100%. Current: {currentAllocation}%, requested: {command.AllocationPercent}%");
            }

            // Calculate cost difference
            var (costWarning, costDifference) = CalculateCostDifference(employee, plannedTeamSlot);

            var assignment = new Domain.Entities.ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = command.ProjectId,
                PlannedTeamSlotId = command.PlannedTeamSlotId,
                EmployeeId = command.EmployeeId,
                StartDate = command.StartDate,
                EndDate = endDate,
                AllocationPercent = command.AllocationPercent,
                Status = AssignmentStatus.Planned, // Always start as Planned (pending approval)
                CostCheckWarning = costWarning,
                CostDifferenceAmount = costDifference,
                Notes = command.Notes,
                RequestedByUserId = userId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = userId,
                ModifiedAt = DateTime.UtcNow
            };

            _context.ActualAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Reload with includes
            var savedAssignment = await _context.ActualAssignments
                .Include(a => a.Project)
                .Include(a => a.PlannedTeamSlot)
                .ThenInclude(pts => pts.Role)
                .Include(a => a.Employee)
                .FirstAsync(a => a.Id == assignment.Id);

            return new AssignmentCreationResult
            {
                Assignment = _mapper.Map<ActualAssignmentDto>(savedAssignment)
            };
        }

        public async Task ApproveAsync(Guid assignmentId, string approverUserId)
        {
            var assignment = await _context.ActualAssignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId && !a.IsDeleted);

            if (assignment == null)
            {
                throw new InvalidOperationException("Assignment not found");
            }

            if (assignment.Status != AssignmentStatus.Planned)
            {
                throw new InvalidOperationException("Only planned assignments can be approved");
            }

            // Recheck allocation limits at approval time
            var allocationAtApproval = await GetEmployeeAllocationAsync(assignment.EmployeeId, assignment.StartDate, assignment.EndDate ?? assignment.StartDate);
            if (allocationAtApproval > 100m)
            {
                throw new InvalidOperationException("Employee allocation would exceed 100% at approval time");
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
            var assignment = await _context.ActualAssignments
                .FirstOrDefaultAsync(a => a.Id == assignmentId && !a.IsDeleted);

            if (assignment == null)
            {
                throw new InvalidOperationException("Assignment not found");
            }

            if (assignment.Status != AssignmentStatus.Planned)
            {
                throw new InvalidOperationException("Only planned assignments can be rejected");
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
            var assignment = await _context.ActualAssignments
                .Include(a => a.Project)
                .FirstOrDefaultAsync(a => a.Id == command.AssignmentId && !a.IsDeleted);

            if (assignment == null)
            {
                throw new InvalidOperationException("Assignment not found");
            }

            if (assignment.Status != AssignmentStatus.Active && assignment.Status != AssignmentStatus.Planned)
            {
                throw new InvalidOperationException("Only active or planned assignments can be unassigned");
            }

            if (command.EndDate < assignment.StartDate || command.EndDate > assignment.Project.EndDate)
            {
                throw new InvalidOperationException("End date must be within project range and after start date");
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
                .Include(a => a.PlannedTeamSlot)
                .ThenInclude(pts => pts.Role)
                .Include(a => a.Employee)
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

        private (bool hasWarning, decimal difference) CalculateCostDifference(Domain.Entities.Employee employee, Domain.Entities.PlannedTeamSlot plannedSlot)
        {
            // Calculate employee cost
            var empCommissionAmount = (employee.CommissionPercent / 100m) * plannedSlot.Project.ProjectPrice;
            var empCostPerMonth = employee.Salary + employee.MonthlyIncentive + empCommissionAmount;

            // Calculate planned cost
            var plannedCommissionAmount = (plannedSlot.PlannedCommissionPercent / 100m) * plannedSlot.Project.ProjectPrice;
            var plannedCostPerMonth = plannedSlot.PlannedSalary + plannedSlot.PlannedIncentive + plannedCommissionAmount;

            var difference = Math.Abs(empCostPerMonth - plannedCostPerMonth);
            var hasWarning = difference > 0.01m; // More than 1 cent difference

            return (hasWarning, difference);
        }
    }
}