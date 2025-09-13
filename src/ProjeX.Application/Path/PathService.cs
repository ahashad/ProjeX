using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Infrastructure.Data;

namespace ProjeX.Application.Path
{
    public class PathService : IPathService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PathService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<PathDto>> GetAllAsync()
        {
            var paths = await _context.Paths
                .Include(p => p.Owner)
                .Include(p => p.Deliverables)
                .Include(p => p.PlannedTeamSlots)
                .Include(p => p.Budgets)
                .Where(p => p.IsActive)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PathDto>>(paths);
        }

        public async Task<IEnumerable<PathDto>> GetByProjectIdAsync(Guid projectId)
        {
            var paths = await _context.Paths
                .Include(p => p.Owner)
                .Include(p => p.Deliverables)
                .Include(p => p.PlannedTeamSlots)
                .Include(p => p.Budgets)
                .Where(p => p.ProjectId == projectId && p.IsActive)
                .ToListAsync();

            return _mapper.Map<IEnumerable<PathDto>>(paths);
        }

        public async Task<PathDto?> GetByIdAsync(Guid id)
        {
            var path = await _context.Paths
                .Include(p => p.Owner)
                .Include(p => p.Deliverables)
                .Include(p => p.PlannedTeamSlots)
                .Include(p => p.Budgets)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            return path != null ? _mapper.Map<PathDto>(path) : null;
        }

        public async Task<PathDto> CreateAsync(CreatePathRequest request)
        {
            // Validate project exists
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            // Validate allocation doesn't exceed 100%
            var isValidAllocation = await ValidatePathAllocationAsync(request.ProjectId, request.AllowedAllocationPercentage);
            if (!isValidAllocation)
                throw new InvalidOperationException("Total path allocation would exceed 100%");

            var path = _mapper.Map<Domain.Entities.Path>(request);
            
            _context.Paths.Add(path);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(path.Id) ?? throw new InvalidOperationException("Failed to retrieve created path");
        }

        public async Task<PathDto> UpdateAsync(UpdatePathRequest request)
        {
            var path = await _context.Paths.FindAsync(request.Id);
            if (path == null)
                throw new ArgumentException("Path not found");

            // Validate allocation doesn't exceed 100%
            var isValidAllocation = await ValidatePathAllocationAsync(path.ProjectId, request.AllowedAllocationPercentage, request.Id);
            if (!isValidAllocation)
                throw new InvalidOperationException("Total path allocation would exceed 100%");

            _mapper.Map(request, path);
            
            await _context.SaveChangesAsync();

            return await GetByIdAsync(path.Id) ?? throw new InvalidOperationException("Failed to retrieve updated path");
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var path = await _context.Paths.FindAsync(id);
            if (path == null)
                return false;

            // Soft delete
            path.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ValidatePathAllocationAsync(Guid projectId, decimal allocationPercentage, Guid? excludePathId = null)
        {
            var totalAllocation = await _context.Paths
                .Where(p => p.ProjectId == projectId && p.IsActive && p.Id != excludePathId)
                .SumAsync(p => p.AllowedAllocationPercentage);

            return (totalAllocation + allocationPercentage) <= 100;
        }

        public async Task<decimal> GetTotalProjectAllocationAsync(Guid projectId)
        {
            return await _context.Paths
                .Where(p => p.ProjectId == projectId && p.IsActive)
                .SumAsync(p => p.AllowedAllocationPercentage);
        }

        public async Task<IEnumerable<PathDto>> GetActivePathsByProjectAsync(Guid projectId)
        {
            return await GetByProjectIdAsync(projectId);
        }
    }
}

