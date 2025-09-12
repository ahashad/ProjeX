using AutoMapper;
using LastMinute.Consultancy.Application.ActualAssignment.Commands;
using LastMinute.Consultancy.Application.Employee;
using LastMinute.Consultancy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Application.ActualAssignment
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
            // Validate PlannedTeamSlot exists and get details
            var plannedTeamSlot = await _context.PlannedTeamSlots
                .Include(pts => pts.Project)
                .Include(pts => pts.Role)
                .FirstOrDefaultAsync(pts => pts.Id == command.PlannedTeamSlotId && !pts.IsDeleted);

            if (plannedTeamSlot == null)
            {
                throw new InvalidOperationException("Planned team slot not found");
            }

            var endDate = command.EndDate ?? plannedTeamSlot.Project.EndDate;

            // Validate date range within project
            if (command.StartDate < plannedTeamSlot.Project.StartDate || endDate > plannedTeamSlot.Project.EndDate)
            {
                throw new InvalidOperationException("Assignment dates must be within project start and end dates");
            }

            if (command.StartDate > endDate)
            {
                throw new InvalidOperationException("Start date cannot be after end date");
            }

            // Check if slot has overlapping assignments
            var overlappingAssignment = await _context.ActualAssignments
                .Include(a => a.Project)
                .Include(a => a.PlannedTeamSlot)
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