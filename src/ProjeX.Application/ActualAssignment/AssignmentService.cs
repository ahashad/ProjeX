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
            var employee = await _context.Employees.FindAsync(command.EmployeeId);
            if (employee == null)
            {
                throw new ArgumentException("Employee not found");
            }

            // Validate planned team slot exists
            var plannedTeamSlot = await _context.PlannedTeamSlots.FindAsync(command.PlannedTeamSlotId);
            if (plannedTeamSlot == null)
            {
                throw new ArgumentException("Planned team slot not found");
            }

            // Create assignment
            var assignment = new Domain.Entities.ActualAssignment
            {
                Id = Guid.NewGuid(),
                ProjectId = command.ProjectId,
                PlannedTeamSlotId = command.PlannedTeamSlotId,
                EmployeeId = command.EmployeeId,
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
                ModifiedAt = DateTime.UtcNow
            };

            _context.ActualAssignments.Add(assignment);
            await _context.SaveChangesAsync();

            var assignmentDto = await GetByIdAsync(assignment.Id);
            result.Assignment = assignmentDto;
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
    }
}