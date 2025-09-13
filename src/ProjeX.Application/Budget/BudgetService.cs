using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Enums;
using ProjeX.Infrastructure.Data;

namespace ProjeX.Application.Budget
{
    public class BudgetService : IBudgetService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BudgetService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BudgetDto>> GetAllAsync()
        {
            var budgets = await _context.Budgets
                .Include(b => b.Project)
                .Include(b => b.Path)
                .Include(b => b.ApprovedBy)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BudgetDto>>(budgets);
        }

        public async Task<IEnumerable<BudgetDto>> GetByProjectIdAsync(Guid projectId)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Project)
                .Include(b => b.Path)
                .Include(b => b.ApprovedBy)
                .Where(b => b.ProjectId == projectId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BudgetDto>>(budgets);
        }

        public async Task<IEnumerable<BudgetDto>> GetByPathIdAsync(Guid pathId)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Project)
                .Include(b => b.Path)
                .Include(b => b.ApprovedBy)
                .Where(b => b.PathId == pathId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BudgetDto>>(budgets);
        }

        public async Task<BudgetDto?> GetByIdAsync(Guid id)
        {
            var budget = await _context.Budgets
                .Include(b => b.Project)
                .Include(b => b.Path)
                .Include(b => b.ApprovedBy)
                .FirstOrDefaultAsync(b => b.Id == id);

            return budget != null ? _mapper.Map<BudgetDto>(budget) : null;
        }

        public async Task<BudgetDto> CreateAsync(CreateBudgetRequest request)
        {
            // Validate project exists
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            // Validate path exists if specified
            if (request.PathId.HasValue)
            {
                var path = await _context.Paths.FindAsync(request.PathId.Value);
                if (path == null || path.ProjectId != request.ProjectId)
                    throw new ArgumentException("Path not found or doesn't belong to the project");
            }

            // Validate dates are within project window
            if (request.PeriodStart < project.StartDate || request.PeriodEnd > project.EndDate)
                throw new InvalidOperationException("Budget period must be within project dates");

            var budget = _mapper.Map<Domain.Entities.Budget>(request);
            
            _context.Budgets.Add(budget);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(budget.Id) ?? throw new InvalidOperationException("Failed to retrieve created budget");
        }

        public async Task<BudgetDto> UpdateAsync(UpdateBudgetRequest request)
        {
            var budget = await _context.Budgets.Include(b => b.Project).FirstOrDefaultAsync(b => b.Id == request.Id);
            if (budget == null)
                throw new ArgumentException("Budget not found");

            // Validate path exists if specified
            if (request.PathId.HasValue)
            {
                var path = await _context.Paths.FindAsync(request.PathId.Value);
                if (path == null || path.ProjectId != budget.ProjectId)
                    throw new ArgumentException("Path not found or doesn't belong to the project");
            }

            // Validate dates are within project window
            if (request.PeriodStart < budget.Project.StartDate || request.PeriodEnd > budget.Project.EndDate)
                throw new InvalidOperationException("Budget period must be within project dates");

            _mapper.Map(request, budget);
            
            await _context.SaveChangesAsync();

            return await GetByIdAsync(budget.Id) ?? throw new InvalidOperationException("Failed to retrieve updated budget");
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
                return false;

            _context.Budgets.Remove(budget);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ApproveBudgetAsync(Guid id, Guid approvedById)
        {
            var budget = await _context.Budgets.FindAsync(id);
            if (budget == null)
                return false;

            budget.IsApproved = true;
            budget.ApprovedById = approvedById;
            budget.ApprovedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<decimal> GetTotalBudgetByProjectAsync(Guid projectId)
        {
            return await _context.Budgets
                .Where(b => b.ProjectId == projectId)
                .SumAsync(b => b.PlannedAmount);
        }

        public async Task<decimal> GetTotalBudgetByCategoryAsync(Guid projectId, BudgetCategory category)
        {
            return await _context.Budgets
                .Where(b => b.ProjectId == projectId && b.Category == category)
                .SumAsync(b => b.PlannedAmount);
        }

        public async Task<IEnumerable<BudgetDto>> GetBudgetsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var budgets = await _context.Budgets
                .Include(b => b.Project)
                .Include(b => b.Path)
                .Include(b => b.ApprovedBy)
                .Where(b => b.PeriodStart <= endDate && b.PeriodEnd >= startDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<BudgetDto>>(budgets);
        }

        public async Task<bool> ValidateBudgetAgainstContractAsync(Guid projectId, decimal additionalAmount)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
                return false;

            var totalBudget = await GetTotalBudgetByProjectAsync(projectId);
            return (totalBudget + additionalAmount) <= project.ContractValue;
        }
    }
}

