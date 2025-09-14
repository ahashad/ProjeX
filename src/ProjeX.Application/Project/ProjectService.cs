using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ProjeX.Application.Project.Commands;
using ProjeX.Infrastructure.Data;
using ProjeX.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ProjeX.Application.Project
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ProjectDto>> GetAllAsync()
        {
            var projects = await _context.Projects
                .Include(p => p.Client)
                .Where(p => !p.IsDeleted)
                .ToListAsync();

            return _mapper.Map<List<ProjectDto>>(projects);
        }

        public async Task<ProjectDto?> GetByIdAsync(Guid id)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

            return project != null ? _mapper.Map<ProjectDto>(project) : null;
        }

        public async Task<ProjectDto> CreateAsync(CreateProjectCommand command, string userId)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == command.ClientId && !c.IsDeleted);

            if (client == null)
                throw new InvalidOperationException("Client not found");

            var entity = new Domain.Entities.Project
            {
                Id = Guid.NewGuid(),
                ProjectName = command.ProjectName,
                ClientId = command.ClientId,
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                Budget = command.Budget,
                ProjectPrice = command.ProjectPrice,
                ExpectedWorkingPeriodMonths = command.ExpectedWorkingPeriodMonths,
                Status = command.Status,
                Notes = command.Notes ?? string.Empty,

                // Set default values for new properties
                Description = string.Empty,
                ContractValue = command.ProjectPrice, // Use project price as default
                Currency = "SAR",
                PaymentTerms = string.Empty,
                PlannedMargin = 0,
                ActualMargin = 0,
                IsApproved = false,

                // Audit fields
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = userId,
                ModifiedAt = DateTime.UtcNow
            };

            _context.Projects.Add(entity);
            await _context.SaveChangesAsync();

            // Reload with client information
            var savedProject = await _context.Projects
                .Include(p => p.Client)
                .FirstAsync(p => p.Id == entity.Id);

            return _mapper.Map<ProjectDto>(savedProject);
        }

        public async Task UpdateAsync(UpdateProjectCommand command, string userId)
        {
            var entity = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == command.Id && !p.IsDeleted);

            if (entity == null)
            {
                throw new InvalidOperationException($"Project with ID {command.Id} not found.");
            }

            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.Id == command.ClientId && !c.IsDeleted);

            if (client == null)
                throw new InvalidOperationException("Client not found");

            // Update basic properties
            entity.ProjectName = command.ProjectName;
            entity.ClientId = command.ClientId;
            entity.StartDate = command.StartDate;
            entity.EndDate = command.EndDate;
            entity.Budget = command.Budget;
            entity.ProjectPrice = command.ProjectPrice;
            entity.ExpectedWorkingPeriodMonths = command.ExpectedWorkingPeriodMonths;
            entity.Status = command.Status;
            entity.Notes = command.Notes ?? string.Empty;

            // Update contract value if it wasn't set before
            if (entity.ContractValue == 0)
            {
                entity.ContractValue = command.ProjectPrice;
            }

            // Update audit fields
            entity.ModifiedBy = userId;
            entity.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Projects.FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Project with ID {id} not found.");
            }

            _context.Projects.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<ProjectDto> ApproveAsync(ApproveProjectCommand command, string userId)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == command.ProjectId && !p.IsDeleted);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {command.ProjectId} not found.");
            }

            if (project.Status != ProjectStatus.Planned)
            {
                throw new InvalidOperationException($"Only planned projects can be approved. Current status: {project.Status}");
            }

            if (command.StartDate >= command.EndDate)
            {
                throw new InvalidOperationException("End date must be after start date.");
            }

            var actualMonths = (decimal)((command.EndDate - command.StartDate).Days / 30.0);
            if (Math.Abs(actualMonths - project.ExpectedWorkingPeriodMonths) > (project.ExpectedWorkingPeriodMonths * 0.5m))
            {
                throw new InvalidOperationException($"Actual project period ({actualMonths:F1} months) deviates significantly from expected period ({project.ExpectedWorkingPeriodMonths} months). Please review the dates or expected period.");
            }

            project.StartDate = command.StartDate;
            project.EndDate = command.EndDate;
            project.ProjectManagerId = command.ProjectManagerId;

            // Set status based on end date
            project.Status = command.EndDate < DateTime.Today ? ProjectStatus.Completed : ProjectStatus.InProgress;
            project.IsApproved = true;
            project.ApprovedAt = command.ApprovedDate;

            var approvalNote = string.IsNullOrEmpty(command.ApprovalNotes)
                ? $"Project approved and started on {command.ApprovedDate:yyyy-MM-dd}"
                : $"Project approved and started on {command.ApprovedDate:yyyy-MM-dd}. Notes: {command.ApprovalNotes}";

            project.Notes = string.IsNullOrEmpty(project.Notes)
                ? approvalNote
                : $"{project.Notes}\n\n{approvalNote}";

            project.ModifiedAt = DateTime.UtcNow;
            project.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            return _mapper.Map<ProjectDto>(project);
        }
    }
}
