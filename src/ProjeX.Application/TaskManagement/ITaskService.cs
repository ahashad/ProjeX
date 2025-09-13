namespace ProjeX.Application.TaskManagement
{
    public interface ITaskService
    {
        Task<TaskDto> CreateAsync(CreateTaskRequest request, string userId);
        Task<TaskDto> UpdateAsync(UpdateTaskRequest request, string userId);
        Task<TaskDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<TaskDto>> GetByDeliverableIdAsync(Guid deliverableId);
        Task<IEnumerable<TaskDto>> GetByEmployeeIdAsync(Guid employeeId);
        Task<bool> DeleteAsync(Guid id);
        Task<CriticalPathDto> GetCriticalPathAsync(Guid deliverableId);
    }
}

