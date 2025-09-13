using AutoMapper;
using ProjeX.Application.RolesCatalog.Commands;
using LastMinute.Consultancy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjeX.Application.RolesCatalog
{
    public class RolesCatalogService : IRolesCatalogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RolesCatalogService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<RolesCatalogDto>> GetAllAsync()
        {
            var roles = await _context.RolesCatalogs.ToListAsync();
            return _mapper.Map<List<RolesCatalogDto>>(roles);
        }

        public async Task<RolesCatalogDto?> GetByIdAsync(Guid id)
        {
            var role = await _context.RolesCatalogs.FirstOrDefaultAsync(r => r.Id == id);
            return role != null ? _mapper.Map<RolesCatalogDto>(role) : null;
        }

        public async Task<RolesCatalogDto> CreateAsync(CreateRolesCatalogCommand command)
        {
            var entity = new Domain.Entities.RolesCatalog
            {
                Id = Guid.NewGuid(),
                RoleName = command.RoleName,
                Level = command.Level,
                DefaultSalary = command.DefaultSalary,
                DefaultMonthlyIncentive = command.DefaultMonthlyIncentive,
                CommissionPercent = command.CommissionPercent,
                Notes = command.Notes
            };

            _context.RolesCatalogs.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<RolesCatalogDto>(entity);
        }

        public async Task UpdateAsync(UpdateRolesCatalogCommand command)
        {
            var entity = await _context.RolesCatalogs.FindAsync(command.Id);
            if (entity == null)
                throw new KeyNotFoundException($"RolesCatalog with ID {command.Id} not found.");

            entity.RoleName = command.RoleName;
            entity.Level = command.Level;
            entity.DefaultSalary = command.DefaultSalary;
            entity.DefaultMonthlyIncentive = command.DefaultMonthlyIncentive;
            entity.CommissionPercent = command.CommissionPercent;
            entity.Notes = command.Notes;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.RolesCatalogs.FindAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"RolesCatalog with ID {id} not found.");

            _context.RolesCatalogs.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
