using AutoMapper;
using ProjeX.Application.PlannedTeamSlot.Commands;
using LastMinute.Consultancy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProjeX.Domain.Enums;

namespace ProjeX.Application.PlannedTeamSlot
{
    public class PlannedTeamSlotService : IPlannedTeamSlotService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<PlannedTeamSlotService> _logger;

        public PlannedTeamSlotService(ApplicationDbContext context, IMapper mapper, ILogger<PlannedTeamSlotService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PlannedTeamSlotDto> CreateSlotAsync(CreatePlannedTeamSlotCommand command, string userId)
        {
            // Validate project exists and get project details
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == command.ProjectId);

            if (project == null)
                throw new InvalidOperationException("Project not found");

            // Validate period months
            if (command.PeriodMonths > project.ExpectedWorkingPeriodMonths)
                throw new InvalidOperationException($"Period months ({command.PeriodMonths}) cannot exceed project expected working period ({project.ExpectedWorkingPeriodMonths} months)");

            // Validate role exists
            var role = await _context.RolesCatalogs
                .FirstOrDefaultAsync(r => r.Id == command.RoleId);

            if (role == null)
                throw new InvalidOperationException("Role not found");

            var slot = new Domain.Entities.PlannedTeamSlot
            {
                Id = Guid.NewGuid(),
                ProjectId = command.ProjectId,
                RoleId = command.RoleId,
                PeriodMonths = command.PeriodMonths,
                AllocationPercent = command.AllocationPercent,
                PlannedSalary = command.PlannedSalary,
                PlannedIncentive = command.PlannedIncentive,
                PlannedCommissionPercent = command.PlannedCommissionPercent,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = userId,
                ModifiedAt = DateTime.UtcNow
            };

            // Calculate budget cost
            slot.ComputedBudgetCost = CalculateBudgetCost(slot, project.ProjectPrice);

            _context.PlannedTeamSlots.Add(slot);
            await _context.SaveChangesAsync();

            // Reload with includes for mapping
            var savedSlot = await _context.PlannedTeamSlots
                .Include(s => s.Project)
                .Include(s => s.Role)
                .FirstAsync(s => s.Id == slot.Id);

            var result = _mapper.Map<PlannedTeamSlotDto>(savedSlot);
            result.IsAssigned = false; // New slot is never assigned initially
            return result;
        }

        public async Task<PlannedTeamSlotDto> UpdateSlotAsync(UpdatePlannedTeamSlotCommand command, string userId)
        {
            var slot = await _context.PlannedTeamSlots
                .Include(s => s.Project)
                .FirstOrDefaultAsync(s => s.Id == command.Id);

            if (slot == null)
                throw new InvalidOperationException("Planned team slot not found");

            // Validate period months
            if (command.PeriodMonths > slot.Project.ExpectedWorkingPeriodMonths)
                throw new InvalidOperationException($"Period months ({command.PeriodMonths}) cannot exceed project expected working period ({slot.Project.ExpectedWorkingPeriodMonths} months)");

            // Check for concurrency conflicts
            if (!slot.RowVersion.SequenceEqual(command.RowVersion))
                throw new InvalidOperationException("The record has been modified by another user. Please refresh and try again.");

            slot.RoleId = command.RoleId;
            slot.PeriodMonths = command.PeriodMonths;
            slot.AllocationPercent = command.AllocationPercent;
            slot.PlannedSalary = command.PlannedSalary;
            slot.PlannedIncentive = command.PlannedIncentive;
            slot.PlannedCommissionPercent = command.PlannedCommissionPercent;
            slot.ModifiedBy = userId;
            slot.ModifiedAt = DateTime.UtcNow;

            // Recalculate budget cost
            slot.ComputedBudgetCost = CalculateBudgetCost(slot, slot.Project.ProjectPrice);

            await _context.SaveChangesAsync();

            // Reload with includes for mapping
            var updatedSlot = await _context.PlannedTeamSlots
                .Include(s => s.Project)
                .Include(s => s.Role)
                .FirstAsync(s => s.Id == slot.Id);

            // Check if this slot is assigned
            var isAssigned = await _context.ActualAssignments
                .AnyAsync(aa => aa.PlannedTeamSlotId == slot.Id && !aa.IsDeleted &&
                                (aa.Status == Domain.Enums.AssignmentStatus.Active ||
                                 aa.Status == Domain.Enums.AssignmentStatus.Planned));

            var result = _mapper.Map<PlannedTeamSlotDto>(updatedSlot);
            result.IsAssigned = isAssigned;
            return result;
        }

        public async Task<PlannedTeamSlotDto?> GetSlotByIdAsync(Guid id)
        {
            var slot = await _context.PlannedTeamSlots
                .Include(s => s.Project)
                .Include(s => s.Role)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

            if (slot == null) return null;

            // Calculate current allocation for this slot
            var allocated = await _context.ActualAssignments
                .Where(aa => aa.PlannedTeamSlotId == id && !aa.IsDeleted &&
                             (aa.Status == AssignmentStatus.Active || aa.Status == AssignmentStatus.Planned))
                .SumAsync(a => a.AllocationPercent);

            var result = _mapper.Map<PlannedTeamSlotDto>(slot);
            result.IsAssigned = allocated > 0;
            result.RemainingAllocationPercent = Math.Max(0, slot.AllocationPercent - allocated);
            return result;
        }

        public async Task<List<PlannedTeamSlotDto>> GetSlotsByProjectAsync(Guid projectId)
        {
            var slots = await _context.PlannedTeamSlots
                .Include(s => s.Project)
                .Include(s => s.Role)
                .Where(s => s.ProjectId == projectId && !s.IsDeleted)
                .ToListAsync();

            // Get allocation per slot in a single query
            var allocations = await _context.ActualAssignments
                .Where(aa => aa.ProjectId == projectId && !aa.IsDeleted &&
                             (aa.Status == AssignmentStatus.Active || aa.Status == AssignmentStatus.Planned))
                .GroupBy(aa => aa.PlannedTeamSlotId)
                .Select(g => new { SlotId = g.Key, Allocated = g.Sum(a => a.AllocationPercent) })
                .ToDictionaryAsync(g => g.SlotId, g => g.Allocated);

            var dtos = _mapper.Map<List<PlannedTeamSlotDto>>(slots);

            foreach (var dto in dtos)
            {
                var allocated = allocations.TryGetValue(dto.Id, out var value) ? value : 0m;
                dto.IsAssigned = allocated > 0;
                dto.RemainingAllocationPercent = Math.Max(0, dto.AllocationPercent - allocated);
            }

            return dtos;
        }

        public async Task<List<PlannedTeamSlotDto>> GetAvailableSlotsAsync(Guid projectId)
        {
            _logger.LogInformation("GetAvailableSlotsAsync called for ProjectId: {ProjectId}", projectId);

            var assignedSlotIds = await _context.ActualAssignments
                .Where(aa => aa.ProjectId == projectId &&
                             (aa.Status == Domain.Enums.AssignmentStatus.Active ||
                              aa.Status == Domain.Enums.AssignmentStatus.Planned))
                .Select(aa => aa.PlannedTeamSlotId)
                .ToListAsync();

            _logger.LogInformation("Found {Count} assigned slots for ProjectId {ProjectId}: {AssignedSlotIds}",
                assignedSlotIds.Count, projectId, string.Join(", ", assignedSlotIds));

            var allProjectSlots = await _context.PlannedTeamSlots
                .Where(s => s.ProjectId == projectId)
                .Select(s => s.Id)
                .ToListAsync();

            _logger.LogInformation("Found {Count} total slots for ProjectId {ProjectId}: {AllProjectSlots}",
                allProjectSlots.Count, projectId, string.Join(", ", allProjectSlots));

            var availableSlots = await _context.PlannedTeamSlots
                .Include(s => s.Project)
                .Include(s => s.Role)
                .Where(s => s.ProjectId == projectId && !assignedSlotIds.Contains(s.Id))
                .ToListAsync();

            _logger.LogInformation("Returning {Count} available slots for ProjectId {ProjectId}", availableSlots.Count, projectId);

            var dtos = _mapper.Map<List<PlannedTeamSlotDto>>(availableSlots);
            foreach (var dto in dtos)
            {
                dto.IsAssigned = false; // Available slots are by definition not assigned
                dto.RemainingAllocationPercent = dto.AllocationPercent;
            }

            return dtos;
        }

        public async Task<Dictionary<Guid, decimal>> GetRemainingAllocationSegmentsAsync(Guid projectId)
        {
            var slots = await _context.PlannedTeamSlots
                .Where(s => s.ProjectId == projectId && !s.IsDeleted)
                .Select(s => new { s.Id, s.AllocationPercent })
                .ToListAsync();

            var assignments = await _context.ActualAssignments
                .Where(a => a.ProjectId == projectId && !a.IsDeleted &&
                            (a.Status == AssignmentStatus.Active || a.Status == AssignmentStatus.Planned))
                .GroupBy(a => a.PlannedTeamSlotId)
                .Select(g => new { SlotId = g.Key, Allocated = g.Sum(a => a.AllocationPercent) })
                .ToListAsync();

            var result = new Dictionary<Guid, decimal>();

            foreach (var slot in slots)
            {
                var allocated = assignments.FirstOrDefault(a => a.SlotId == slot.Id)?.Allocated ?? 0m;
                var remaining = Math.Max(0, slot.AllocationPercent - allocated);
                result[slot.Id] = remaining;
            }

            return result;
        }

        public async Task DeleteSlotAsync(Guid id, string userId)
        {
            var slot = await _context.PlannedTeamSlots.FindAsync(id);
            if (slot == null)
                throw new InvalidOperationException("Planned team slot not found");

            // Check if slot has any assignments
            var hasAssignments = await _context.ActualAssignments
                .AnyAsync(aa => aa.PlannedTeamSlotId == id && !aa.IsDeleted);

            if (hasAssignments)
                throw new InvalidOperationException("Cannot delete slot that has actual assignments");

            slot.IsDeleted = true;
            slot.ModifiedBy = userId;
            slot.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task RecalculateBudgetCostsAsync(Guid projectId)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return;

            var slots = await _context.PlannedTeamSlots
                .Where(s => s.ProjectId == projectId)
                .ToListAsync();

            foreach (var slot in slots)
            {
                slot.ComputedBudgetCost = CalculateBudgetCost(slot, project.ProjectPrice);
            }

            await _context.SaveChangesAsync();
        }

        private decimal CalculateBudgetCost(Domain.Entities.PlannedTeamSlot slot, decimal projectPrice)
        {
            var plannedCommissionAmount = (slot.PlannedCommissionPercent / 100m) * projectPrice;
            var plannedCostPerMonth = slot.PlannedSalary + slot.PlannedIncentive + plannedCommissionAmount;
            return plannedCostPerMonth * slot.PeriodMonths * (slot.AllocationPercent / 100m);
        }
    }
}