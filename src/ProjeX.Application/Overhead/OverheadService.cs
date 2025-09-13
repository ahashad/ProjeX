using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ProjeX.Application.Overhead.Commands;
using ProjeX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjeX.Application.Overhead
{
    public class OverheadService : IOverheadService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public OverheadService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<OverheadDto>> GetAllAsync()
        {
            var overheads = await _context.Overheads
                .Include(o => o.Project)
                .ToListAsync();

            return _mapper.Map<List<OverheadDto>>(overheads);
        }

        public async Task<OverheadDto?> GetByIdAsync(Guid id)
        {
            var overhead = await _context.Overheads
                .Include(o => o.Project)
                .FirstOrDefaultAsync(o => o.Id == id);

            return overhead != null ? _mapper.Map<OverheadDto>(overhead) : null;
        }

        public async Task<OverheadDto> CreateAsync(CreateOverheadCommand command, string userId)
        {
            var entity = new Domain.Entities.Overhead
            {
                Id = Guid.NewGuid(),
                Description = command.Description,
                Amount = command.Amount,
                Date = command.Date,
                Category = command.Category,
                ProjectId = command.ProjectId,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = userId,
                ModifiedAt = DateTime.UtcNow
            };

            _context.Overheads.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<OverheadDto>(entity);
        }

        public async Task UpdateAsync(UpdateOverheadCommand command, string userId)
        {
            var entity = await _context.Overheads.FindAsync(command.Id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Overhead with ID {command.Id} not found.");
            }

            entity.Description = command.Description;
            entity.Amount = command.Amount;
            entity.Date = command.Date;
            entity.Category = command.Category;
            entity.ProjectId = command.ProjectId;
            entity.ModifiedBy = userId;
            entity.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Overheads.FindAsync(id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Overhead with ID {id} not found.");
            }

            _context.Overheads.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
