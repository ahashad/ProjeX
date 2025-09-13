using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjeX.Application.Project.Commands;

namespace ProjeX.Application.Project
{
    public interface IProjectService
    {
        Task<List<ProjectDto>> GetAllAsync();
        Task<ProjectDto?> GetByIdAsync(Guid id);
        Task<ProjectDto> CreateAsync(CreateProjectCommand command);
        Task UpdateAsync(UpdateProjectCommand command);
        Task DeleteAsync(Guid id);
        Task<ProjectDto> ApproveAsync(ApproveProjectCommand command);
    }
}
