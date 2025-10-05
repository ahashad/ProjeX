using AutoMapper;
using ProjeX.Application.PlannedTeamSlot.Commands;
using ProjeX.Infrastructure.Data;
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
                PlannedTickets = command.PlannedTickets,
                PlannedHoteling = command.PlannedHoteling,
                PlannedOthers = command.PlannedOthers,
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
            slot.PlannedTickets = command.PlannedTickets;
            slot.PlannedHoteling = command.PlannedHoteling;
            slot.PlannedOthers = command.PlannedOthers;
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

            // Get project for cost calculations
            var project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new InvalidOperationException("Project not found");

            // Get allocation per slot in a single query
            var allocations = slots
                .Where(aa => aa.ProjectId == projectId && !aa.IsDeleted &&
                             aa.Status != PlannedTeamStatus.Cancelled)
                .GroupBy(aa => aa.Id)
                .Select(g => new { SlotId = g.Key, Allocated = g.Sum(a => a.AllocationPercent) })
                .ToDictionary(g => g.SlotId, g => g.Allocated);

            // Get all assignments for cost calculation
            var assignments = await _context.ActualAssignments
                .AsNoTracking()
                .Include(aa => aa.Employee)
                .Where(aa => aa.ProjectId == projectId && !aa.IsDeleted &&
                             aa.Status != AssignmentStatus.Cancelled)
                .ToListAsync();

            // Calculate actual costs and utilization per slot
            var slotActualCosts = new Dictionary<Guid, decimal>();
            var slotAssignmentDurations = new Dictionary<Guid, decimal>(); // Track total duration in days per slot

            // NetProjectPrice for commission calculations
            var netProjectPrice = project.ProjectPrice / 1.15m;

            foreach (var assignment in assignments)
            {
                if (!assignment.PlannedTeamSlotId.HasValue)
                    continue;

                var slotId = assignment.PlannedTeamSlotId.Value;

                // Use snapshot values if available, otherwise fall back to employee values
                var salary = assignment.SnapshotSalary ?? assignment.Employee?.Salary ?? 0;
                var monthlyIncentive = assignment.SnapshotMonthlyIncentive ?? assignment.Employee?.MonthlyIncentive ?? 0;
                var commissionPercent = assignment.SnapshotCommissionPercent ?? assignment.Employee?.CommissionPercent ?? 0;
                var tickets = assignment.SnapshotTickets ?? 0;
                var hoteling = assignment.SnapshotHoteling ?? 0;
                var others = assignment.SnapshotOthers ?? 0;

                // Calculate assignment duration in days and months
                var endDate = assignment.EndDate ?? DateTime.Today;
                var durationDays = (endDate - assignment.StartDate).Days;
                var monthsWorked = durationDays / 30.0m;

                // Calculate commission amount (one-time, using NetProjectPrice)
                var commissionAmount = (commissionPercent / 100m) * netProjectPrice;

                // Calculate total actual cost for this assignment
                var assignmentActualCost = ((salary + monthlyIncentive + tickets + hoteling + others) * monthsWorked) + commissionAmount;

                // Aggregate costs to slot level
                if (slotActualCosts.ContainsKey(slotId))
                    slotActualCosts[slotId] += assignmentActualCost;
                else
                    slotActualCosts[slotId] = assignmentActualCost;

                // Aggregate assignment durations for utilization calculation
                if (slotAssignmentDurations.ContainsKey(slotId))
                    slotAssignmentDurations[slotId] += durationDays;
                else
                    slotAssignmentDurations[slotId] = durationDays;
            }

            // Calculate utilization per slot: (sum of assignment durations / slot period in days) * 100
            var slotUtilizations = new Dictionary<Guid, decimal>();
            foreach (var slot in slots)
            {
                var slotPeriodDays = slot.PeriodMonths * 30m; // Convert months to days
                var totalAssignmentDays = slotAssignmentDurations.TryGetValue(slot.Id, out var days) ? days : 0m;

                slotUtilizations[slot.Id] = slotPeriodDays > 0
                    ? (totalAssignmentDays / slotPeriodDays) * 100m
                    : 0m;
            }

            var dtos = _mapper.Map<List<PlannedTeamSlotDto>>(slots);

            foreach (var dto in dtos)
            {
                var allocated = allocations.TryGetValue(dto.Id, out var value) ? value : 0m;
                dto.IsAssigned = allocated > 0;
                dto.RemainingAllocationPercent = Math.Max(0, dto.AllocationPercent - allocated);

                // Set cost fields
                dto.PlannedCost = dto.ComputedBudgetCost; // Planned cost is the budget cost
                dto.ActualCost = slotActualCosts.TryGetValue(dto.Id, out var actualCost) ? actualCost : 0m;
                dto.Variance = dto.ActualCost - dto.PlannedCost;

                // Set utilization field (sum of all assignments' utilization under this slot)
                dto.UtilizationPercent = slotUtilizations.TryGetValue(dto.Id, out var utilization) ? utilization : 0m;
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

        public async Task<TeamPlanningKpiDto> GetProjectKpisAsync(Guid projectId)
        {
            var startTime = DateTime.UtcNow;

            // Get project for cost calculations
            var project = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null)
                throw new InvalidOperationException("Project not found");

            // Get all planned slots with their data
            var slots = await _context.PlannedTeamSlots
                .AsNoTracking()
                .Where(s => s.ProjectId == projectId && !s.IsDeleted)
                .ToListAsync();

            // Get all assignments for this project (Active, Planned, and Completed for counts and costs)
            var assignments = await _context.ActualAssignments
                .AsNoTracking()
                .Include(aa => aa.Employee)
                .Where(aa => aa.ProjectId == projectId && !aa.IsDeleted)
                .ToListAsync();

            // Calculate project total days for utilization
            var projectStartDate = project.StartDate ?? DateTime.Today;
            var projectEndDate = project.EndDate ?? DateTime.Today;
            var totalDaysInProject = (projectEndDate - projectStartDate).Days;

            // Initialize KPI aggregators
            var kpi = new TeamPlanningKpiDto
            {
                CalculatedAt = DateTime.UtcNow,
                PlannedSlotsCount = slots.Count
            };

            // Calculate cost KPIs per slot
            var slotActualCosts = new Dictionary<Guid, decimal>();
            var slotUtilizations = new Dictionary<Guid, decimal>();

            foreach (var assignment in assignments.Where(a => a.Status == AssignmentStatus.Active || a.Status == AssignmentStatus.Completed))
            {
                if (!assignment.PlannedTeamSlotId.HasValue)
                    continue;

                var slotId = assignment.PlannedTeamSlotId.Value;

                // Use snapshot values if available, otherwise fall back to employee values
                var salary = assignment.SnapshotSalary ?? assignment.Employee?.Salary ?? 0;
                var monthlyIncentive = assignment.SnapshotMonthlyIncentive ?? assignment.Employee?.MonthlyIncentive ?? 0;
                var commissionPercent = assignment.SnapshotCommissionPercent ?? assignment.Employee?.CommissionPercent ?? 0;
                var tickets = assignment.SnapshotTickets ?? 0;
                var hoteling = assignment.SnapshotHoteling ?? 0;
                var others = assignment.SnapshotOthers ?? 0;

                // Calculate assignment duration
                var endDate = assignment.EndDate ?? DateTime.Today;
                var durationDays = (endDate - assignment.StartDate).Days;
                var monthsWorked = durationDays / 30.0m;

                // Calculate commission amount
                var commissionAmount = (commissionPercent / 100m) * project.ProjectPrice;

                // Calculate total actual cost for this assignment
                var assignmentActualCost = (salary + monthlyIncentive + commissionAmount + tickets + hoteling + others) * monthsWorked;

                // Aggregate costs to slot level
                if (slotActualCosts.ContainsKey(slotId))
                    slotActualCosts[slotId] += assignmentActualCost;
                else
                    slotActualCosts[slotId] = assignmentActualCost;

                // Calculate utilization percentage
                var utilizationPercent = totalDaysInProject > 0
                    ? (assignment.AllocationPercent * durationDays) / (decimal)totalDaysInProject
                    : 0m;

                if (slotUtilizations.ContainsKey(slotId))
                    slotUtilizations[slotId] += utilizationPercent;
                else
                    slotUtilizations[slotId] = utilizationPercent;
            }

            // Aggregate slot-level data to project-level KPIs
            // NetProjectPrice for commission calculations
            var netProjectPrice = project.ProjectPrice * 0.85m;

            foreach (var slot in slots)
            {
                // Calculate planned cost for this slot using canonical formula
                var plannedCommissionAmount = (slot.PlannedCommissionPercent / 100m) * netProjectPrice;
                var plannedCostPerMonth = slot.PlannedSalary + slot.PlannedIncentive +
                                         slot.PlannedTickets + slot.PlannedHoteling + slot.PlannedOthers;
                var slotPlannedCost = (plannedCostPerMonth * slot.PeriodMonths) + plannedCommissionAmount;

                kpi.TotalPlannedCost += slotPlannedCost;
                kpi.PlannedAllocationPercent += slot.AllocationPercent;

                // Add actual cost if exists
                if (slotActualCosts.TryGetValue(slot.Id, out var actualCost))
                {
                    kpi.TotalActualCost += actualCost;
                }
            }

            // Calculate variance
            kpi.TotalVariance = kpi.TotalActualCost - kpi.TotalPlannedCost;
            kpi.VariancePercentOfPlanned = kpi.TotalPlannedCost > 0
                ? (kpi.TotalVariance / kpi.TotalPlannedCost) * 100
                : 0;

            // Calculate allocation metrics
            var activeAndPlannedAllocations = assignments
                .Where(a => a.Status == AssignmentStatus.Active || a.Status == AssignmentStatus.Planned)
                .Sum(a => a.AllocationPercent);

            kpi.UtilizedAllocationPercent = activeAndPlannedAllocations;
            kpi.RemainingAllocationPercent = Math.Max(0, kpi.PlannedAllocationPercent - activeAndPlannedAllocations);

            // Count assignments by status
            kpi.ActiveAssignmentsCount = assignments.Count(a => a.Status == AssignmentStatus.Active);
            kpi.CompletedAssignmentsCount = assignments.Count(a => a.Status == AssignmentStatus.Completed);

            // Calculate average utilization
            if (slotUtilizations.Any())
            {
                kpi.AverageUtilizationPercent = slotUtilizations.Values.Average();
            }

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("TeamPlanning.KPI.Loaded - ProjectId: {ProjectId}, SlotsCount: {SlotsCount}, Duration: {Duration}ms",
                projectId, kpi.PlannedSlotsCount, duration);

            return kpi;
        }

        private decimal CalculateBudgetCost(Domain.Entities.PlannedTeamSlot slot, decimal projectPrice)
        {
            // NetProjectPrice = ProjectPrice after deducting 15% VAT
            var netProjectPrice = projectPrice / 1.15m;

            // Commission is total one-time for the project, not monthly
            var plannedCommissionAmount = (slot.PlannedCommissionPercent / 100m) * netProjectPrice;

            // Monthly costs (salary, incentive, expenses)
            var plannedCostPerMonth = slot.PlannedSalary + slot.PlannedIncentive +
                                      slot.PlannedTickets + slot.PlannedHoteling + slot.PlannedOthers;

            // Total cost = (monthly costs * period) + one-time commission
            return (plannedCostPerMonth * slot.PeriodMonths) + plannedCommissionAmount;
        }
    }
}