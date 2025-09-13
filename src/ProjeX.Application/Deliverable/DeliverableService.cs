using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ProjeX.Application.Deliverable.Commands;
using ProjeX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjeX.Application.Deliverable
{
    public class DeliverableService : IDeliverableService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public DeliverableService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<DeliverableDto>> GetAllAsync()
        {
            var deliverables = await _context.Deliverables
                .Include(d => d.Project)
                .ToListAsync();

            return _mapper.Map<List<DeliverableDto>>(deliverables);
        }

        public async Task<DeliverableDto?> GetByIdAsync(Guid id)
        {
            var deliverable = await _context.Deliverables
                .Include(d => d.Project)
                .FirstOrDefaultAsync(d => d.Id == id);

            return deliverable != null ? _mapper.Map<DeliverableDto>(deliverable) : null;
        }

        public async Task<DeliverableDto> CreateAsync(CreateDeliverableCommand command)
        {
            var entity = new Domain.Entities.Deliverable
            {
                Id = Guid.NewGuid(),
                ProjectId = command.ProjectId,
                Name = command.Title,
                Description = command.Description,
                DueDate = command.DueDate,
                Status = command.Status
            };

            _context.Deliverables.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<DeliverableDto>(entity);
        }

        public async Task UpdateAsync(UpdateDeliverableCommand command)
        {
            var entity = await _context.Deliverables.FindAsync(command.Id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Deliverable with ID {command.Id} not found.");
            }

            entity.ProjectId = command.ProjectId;
            entity.Name = command.Title;
            entity.Description = command.Description;
            entity.DueDate = command.DueDate;
            entity.Status = command.Status;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Deliverables.FindAsync(id);

            if (entity == null)
            {
                throw new KeyNotFoundException($"Deliverable with ID {id} not found.");
            }

            _context.Deliverables.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
