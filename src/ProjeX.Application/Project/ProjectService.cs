using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ProjeX.Application.Project.Commands;
using LastMinute.Consultancy.Infrastructure.Data;
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
            var projectData = await _context.Projects
                .Include(p => p.Client)
                .Select(p => new
                {
                    p.Id,
                    p.ProjectName,
                    p.ClientId,
                    ClientName = p.Client.ClientName,
                    p.StartDate,
                    p.EndDate,
                    p.Budget,
                    p.ProjectPrice,
                    ExpectedWorkingPeriodMonths = EF.Property<int>(p, "ExpectedWorkingPeriodMonths") == 0
                        ? 0m
                        : (decimal)EF.Property<int>(p, "ExpectedWorkingPeriodMonths"),
                    p.Status,
                    p.Notes,
                    p.CreatedBy,
                    p.CreatedAt,
                    p.ModifiedBy,
                    p.ModifiedAt
                })
                .ToListAsync();

            return projectData.Select(p => new ProjectDto
            {
                Id = p.Id,
                ProjectName = p.ProjectName,
                ClientId = p.ClientId,
                ClientName = p.ClientName,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Budget = p.Budget,
                ProjectPrice = p.ProjectPrice,
                ExpectedWorkingPeriodMonths = p.ExpectedWorkingPeriodMonths,
                Status = p.Status,
                Notes = p.Notes,
                CreatedBy = p.CreatedBy,
                CreatedAt = p.CreatedAt,
                ModifiedBy = p.ModifiedBy,
                ModifiedAt = p.ModifiedAt
            }).ToList();
        }

        public async Task<ProjectDto?> GetByIdAsync(Guid id)
        {
            var projectData = await _context.Projects
                .Include(p => p.Client)
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.ProjectName,
                    p.ClientId,
                    ClientName = p.Client.ClientName,
                    p.StartDate,
                    p.EndDate,
                    p.Budget,
                    p.ProjectPrice,
                    ExpectedWorkingPeriodMonths = EF.Property<int>(p, "ExpectedWorkingPeriodMonths") == 0
                        ? 0m
                        : (decimal)EF.Property<int>(p, "ExpectedWorkingPeriodMonths"),
                    p.Status,
                    p.Notes,
                    p.CreatedBy,
                    p.CreatedAt,
                    p.ModifiedBy,
                    p.ModifiedAt
                })
                .FirstOrDefaultAsync();

            if (projectData == null)
            {
                return null;
            }

            return new ProjectDto
            {
                Id = projectData.Id,
                ProjectName = projectData.ProjectName,
                ClientId = projectData.ClientId,
                ClientName = projectData.ClientName,
                StartDate = projectData.StartDate,
                EndDate = projectData.EndDate,
                Budget = projectData.Budget,
                ProjectPrice = projectData.ProjectPrice,
                ExpectedWorkingPeriodMonths = projectData.ExpectedWorkingPeriodMonths,
                Status = projectData.Status,
                Notes = projectData.Notes,
                CreatedBy = projectData.CreatedBy,
                CreatedAt = projectData.CreatedAt,
                ModifiedBy = projectData.ModifiedBy,
                ModifiedAt = projectData.ModifiedAt
            };
        }

        public async Task<ProjectDto> CreateAsync(CreateProjectCommand command)
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
                StartDate = DateTime.MinValue,
                EndDate = DateTime.MinValue,
                Budget = command.Budget,
                ProjectPrice = command.ProjectPrice,
                Status = command.Status,
                Notes = command.Notes,
                CreatedBy = "current-user",
                CreatedAt = DateTime.UtcNow,
                ModifiedBy = "current-user",
                ModifiedAt = DateTime.UtcNow
            };

            try
            {
                _context.Projects.Add(entity);
                _context.Entry(entity).Property("ExpectedWorkingPeriodMonths").CurrentValue =
                    Convert.ToInt32(Math.Round(command.ExpectedWorkingPeriodMonths));
            }
            catch
            {
                entity.ExpectedWorkingPeriodMonths = command.ExpectedWorkingPeriodMonths;
                _context.Projects.Add(entity);
            }

            await _context.SaveChangesAsync();

            var savedProjectData = await _context.Projects
                .Include(p => p.Client)
                .Where(p => p.Id == entity.Id)
                .Select(p => new
                {
                    p.Id,
                    p.ProjectName,
                    p.ClientId,
                    ClientName = p.Client.ClientName,
                    p.StartDate,
                    p.EndDate,
                    p.Budget,
                    p.ProjectPrice,
                    ExpectedWorkingPeriodMonths = EF.Property<int>(p, "ExpectedWorkingPeriodMonths") == 0
                        ? 0m
                        : (decimal)EF.Property<int>(p, "ExpectedWorkingPeriodMonths"),
                    p.Status,
                    p.Notes,
                    p.CreatedBy,
                    p.CreatedAt,
                    p.ModifiedBy,
                    p.ModifiedAt
                })
                .FirstAsync();

            return new ProjectDto
            {
                Id = savedProjectData.Id,
                ProjectName = savedProjectData.ProjectName,
                ClientId = savedProjectData.ClientId,
                ClientName = savedProjectData.ClientName,
                StartDate = savedProjectData.StartDate,
                EndDate = savedProjectData.EndDate,
                Budget = savedProjectData.Budget,
                ProjectPrice = savedProjectData.ProjectPrice,
                ExpectedWorkingPeriodMonths = savedProjectData.ExpectedWorkingPeriodMonths,
                Status = savedProjectData.Status,
                Notes = savedProjectData.Notes,
                CreatedBy = savedProjectData.CreatedBy,
                CreatedAt = savedProjectData.CreatedAt,
                ModifiedBy = savedProjectData.ModifiedBy,
                ModifiedAt = savedProjectData.ModifiedAt
            };
        }

        public async Task UpdateAsync(UpdateProjectCommand command)
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

            entity.ProjectName = command.ProjectName;
            entity.ClientId = command.ClientId;
            entity.StartDate = command.StartDate;
            entity.EndDate = command.EndDate;
            entity.Budget = command.Budget;
            entity.ProjectPrice = command.ProjectPrice;

            try
            {
                _context.Entry(entity).Property("ExpectedWorkingPeriodMonths").CurrentValue =
                    Convert.ToInt32(Math.Round(command.ExpectedWorkingPeriodMonths));
            }
            catch
            {
                entity.ExpectedWorkingPeriodMonths = command.ExpectedWorkingPeriodMonths;
            }

            entity.Status = command.Status;
            entity.Notes = command.Notes;
            entity.ModifiedBy = "current-user";
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

        public async Task<ProjectDto> ApproveAsync(ApproveProjectCommand command)
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

            decimal expectedPeriod;
            try
            {
                var intValue = _context.Entry(project).Property("ExpectedWorkingPeriodMonths").CurrentValue;
                expectedPeriod = Convert.ToDecimal(intValue);
            }
            catch
            {
                expectedPeriod = project.ExpectedWorkingPeriodMonths;
            }

            var actualMonths = (decimal)((command.EndDate - command.StartDate).Days / 30.0);
            if (Math.Abs(actualMonths - expectedPeriod) > (expectedPeriod * 0.5m))
            {
                throw new InvalidOperationException($"Actual project period ({actualMonths:F1} months) deviates significantly from expected period ({expectedPeriod} months). Please review the dates or expected period.");
            }

            project.StartDate = command.StartDate;
            project.EndDate = command.EndDate;
            project.Status = ProjectStatus.InProgress;

            var approvalNote = string.IsNullOrEmpty(command.ApprovalNotes)
                ? $"Project approved and started on {command.ApprovedDate:yyyy-MM-dd}"
                : $"Project approved and started on {command.ApprovedDate:yyyy-MM-dd}. Notes: {command.ApprovalNotes}";

            project.Notes = string.IsNullOrEmpty(project.Notes)
                ? approvalNote
                : $"{project.Notes}\n\n{approvalNote}";

            project.ModifiedAt = DateTime.UtcNow;
            project.ModifiedBy = "current-user";

            await _context.SaveChangesAsync();

            return new ProjectDto
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                ClientId = project.ClientId,
                ClientName = project.Client.ClientName,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Budget = project.Budget,
                ProjectPrice = project.ProjectPrice,
                ExpectedWorkingPeriodMonths = expectedPeriod,
                Status = project.Status,
                Notes = project.Notes,
                CreatedBy = project.CreatedBy,
                CreatedAt = project.CreatedAt,
                ModifiedBy = project.ModifiedBy,
                ModifiedAt = project.ModifiedAt
            };
        }
    }
}
