using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Enums;
using ProjeX.Infrastructure.Data;
using TaskStatus = ProjeX.Domain.Enums.TaskStatus;

namespace ProjeX.Application.TaskManagement
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TaskService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<TaskDto> CreateAsync(CreateTaskRequest request)
        {
            // Validate deliverable exists
            var deliverable = await _context.Deliverables.FindAsync(request.DeliverableId);
            if (deliverable == null)
                throw new ArgumentException("Deliverable not found");

            // Validate dependencies exist
            if (request.DependentTaskIds?.Any() == true)
            {
                var dependentTasks = await _context.Tasks
                    .Where(t => request.DependentTaskIds.Contains(t.Id))
                    .CountAsync();
                
                if (dependentTasks != request.DependentTaskIds.Count)
                    throw new ArgumentException("One or more dependent tasks not found");
            }

            var task = _mapper.Map<Domain.Entities.Task>(request);
            task.Id = Guid.NewGuid();
            task.Status = TaskStatus.NotStarted;

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Add dependencies
            if (request.DependentTaskIds?.Any() == true)
            {
                foreach (var dependentTaskId in request.DependentTaskIds)
                {
                    _context.TaskDependencies.Add(new Domain.Entities.TaskDependency
                    {
                        TaskId = task.Id,
                        DependentTaskId = dependentTaskId
                    });
                }
                await _context.SaveChangesAsync();
            }

            return await GetByIdAsync(task.Id) ?? throw new InvalidOperationException("Failed to retrieve created task");
        }

        public async Task<TaskDto> UpdateAsync(UpdateTaskRequest request)
        {
            var task = await _context.Tasks.FindAsync(request.Id);
            if (task == null)
                throw new ArgumentException("Task not found");

            // Validate status transitions
            if (!IsValidStatusTransition(task.Status, request.Status))
                throw new InvalidOperationException($"Invalid status transition from {task.Status} to {request.Status}");

            var oldStatus = task.Status;
            _mapper.Map(request, task);

            // Update progress based on status
            if (request.Status == TaskStatus.Completed && task.ProgressPercentage < 100)
            {
                task.ProgressPercentage = 100;
                task.ActualEndDate = DateTime.UtcNow;
            }
            else if (request.Status == TaskStatus.InProgress && oldStatus == TaskStatus.NotStarted)
            {
                task.ActualStartDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Check if deliverable should be updated
            await UpdateDeliverableProgressAsync(task.DeliverableId);

            return await GetByIdAsync(task.Id) ?? throw new InvalidOperationException("Failed to retrieve updated task");
        }

        public async Task<TaskDto?> GetByIdAsync(Guid id)
        {
            var task = await _context.Tasks
                .Include(t => t.Deliverable)
                .Include(t => t.AssignedEmployee)
                // TODO: Add back Dependencies include after fixing EF configuration
                // .Include(t => t.Dependencies)
                //     .ThenInclude(d => d.DependentTask)
                .Include(t => t.Approvals)
                .FirstOrDefaultAsync(t => t.Id == id);

            return task != null ? _mapper.Map<TaskDto>(task) : null;
        }

        public async Task<IEnumerable<TaskDto>> GetByDeliverableIdAsync(Guid deliverableId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Deliverable)
                .Include(t => t.AssignedEmployee)
                // TODO: Add back Dependencies include after fixing EF configuration
                // .Include(t => t.Dependencies)
                //     .ThenInclude(d => d.DependentTask)
                .Include(t => t.Approvals)
                .Where(t => t.DeliverableId == deliverableId)
                .OrderBy(t => t.PlannedStartDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<IEnumerable<TaskDto>> GetByEmployeeIdAsync(Guid employeeId)
        {
            var tasks = await _context.Tasks
                .Include(t => t.Deliverable)
                    .ThenInclude(d => d.Project)
                .Include(t => t.AssignedEmployee)
                .Where(t => t.AssignedEmployeeId == employeeId && t.Status != TaskStatus.Completed)
                .OrderBy(t => t.PlannedStartDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TaskDto>>(tasks);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var task = await _context.Tasks
                // TODO: Add back Dependencies include after fixing EF configuration
                // .Include(t => t.Dependencies)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (task == null)
                return false;

            // Check if task has dependent tasks
            var hasDependents = await _context.TaskDependencies
                .AnyAsync(td => td.DependentTaskId == id);
            
            if (hasDependents)
                throw new InvalidOperationException("Cannot delete task that has dependent tasks");

            // Remove dependencies
            // TODO: Fix Dependencies removal after fixing EF configuration
            // _context.TaskDependencies.RemoveRange(task.Dependencies);
            var taskDependencies = await _context.TaskDependencies.Where(td => td.TaskId == id).ToListAsync();
            _context.TaskDependencies.RemoveRange(taskDependencies);
            
            // Remove task
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            // Update deliverable progress
            await UpdateDeliverableProgressAsync(task.DeliverableId);

            return true;
        }

        public async Task<CriticalPathDto> GetCriticalPathAsync(Guid deliverableId)
        {
            var tasks = await _context.Tasks
                // TODO: Add back Dependencies include after fixing EF configuration
                // .Include(t => t.Dependencies)
                //     .ThenInclude(d => d.DependentTask)
                .Where(t => t.DeliverableId == deliverableId)
                .ToListAsync();

            if (!tasks.Any())
                return new CriticalPathDto { DeliverableId = deliverableId };

            // Calculate critical path using forward and backward pass
            var criticalPath = CalculateCriticalPath(tasks);
            var totalDuration = criticalPath.Sum(t => t.EstimatedHours);
            var earliestCompletion = criticalPath.Max(t => t.PlannedEndDate);

            return new CriticalPathDto
            {
                DeliverableId = deliverableId,
                CriticalTasks = _mapper.Map<List<TaskDto>>(criticalPath),
                TotalDuration = totalDuration,
                EarliestCompletionDate = earliestCompletion
            };
        }

        private async Task UpdateDeliverableProgressAsync(Guid deliverableId)
        {
            var tasks = await _context.Tasks
                .Where(t => t.DeliverableId == deliverableId)
                .ToListAsync();

            if (!tasks.Any()) return;

            var totalWeight = tasks.Sum(t => t.EstimatedHours);
            var completedWeight = tasks
                .Where(t => t.Status == TaskStatus.Completed)
                .Sum(t => t.EstimatedHours);

            var progressPercentage = totalWeight > 0 ? (completedWeight / totalWeight) * 100 : 0;

            var deliverable = await _context.Deliverables.FindAsync(deliverableId);
            if (deliverable != null)
            {
                deliverable.ProgressPercentage = (int)Math.Round(progressPercentage);
                
                // Update status based on progress
                if (progressPercentage >= 100)
                {
                    deliverable.Status = DeliverableStatus.Completed;
                    deliverable.ActualEndDate = DateTime.UtcNow;
                }
                else if (progressPercentage > 0)
                {
                    deliverable.Status = DeliverableStatus.InProgress;
                    if (deliverable.ActualStartDate == null)
                        deliverable.ActualStartDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
            }
        }

        private bool IsValidStatusTransition(ProjeX.Domain.Enums.TaskStatus currentStatus, ProjeX.Domain.Enums.TaskStatus newStatus)
        {
            return currentStatus switch
            {
                TaskStatus.NotStarted => newStatus == TaskStatus.InProgress || newStatus == TaskStatus.Cancelled,
                TaskStatus.InProgress => newStatus == TaskStatus.Completed || newStatus == TaskStatus.OnHold || newStatus == TaskStatus.Cancelled,
                TaskStatus.OnHold => newStatus == TaskStatus.InProgress || newStatus == TaskStatus.Cancelled,
                TaskStatus.Completed => false, // Cannot change from completed
                TaskStatus.Cancelled => newStatus == TaskStatus.NotStarted, // Can restart cancelled tasks
                _ => false
            };
        }

        private List<Domain.Entities.Task> CalculateCriticalPath(List<Domain.Entities.Task> tasks)
        {
            // Simplified critical path calculation
            // In a real implementation, this would use proper CPM algorithm
            
            var taskDict = tasks.ToDictionary(t => t.Id, t => t);
            var criticalTasks = new List<Domain.Entities.Task>();

            // Find tasks with no dependencies (start tasks)
            // TODO: Fix Dependencies check after fixing EF configuration
            // var startTasks = tasks.Where(t => !t.Dependencies.Any()).ToList();
            // For now, return all tasks as a simplified critical path
            var startTasks = tasks.ToList();
            
            // For simplicity, return the longest path by duration
            var longestPath = new List<Domain.Entities.Task>();
            decimal maxDuration = 0;

            foreach (var startTask in startTasks)
            {
                var path = GetLongestPath(startTask, taskDict, new HashSet<Guid>());
                var pathDuration = path.Sum(t => t.EstimatedHours);
                
                if (pathDuration > maxDuration)
                {
                    maxDuration = pathDuration;
                    longestPath = path;
                }
            }

            return longestPath;
        }

        private List<Domain.Entities.Task> GetLongestPath(Domain.Entities.Task task, Dictionary<Guid, Domain.Entities.Task> taskDict, HashSet<Guid> visited)
        {
            if (visited.Contains(task.Id))
                return new List<Domain.Entities.Task>(); // Avoid cycles

            visited.Add(task.Id);
            var path = new List<Domain.Entities.Task> { task };

            var dependentTasks = _context.TaskDependencies
                .Where(td => td.DependentTaskId == task.Id)
                .Select(td => taskDict[td.TaskId])
                .ToList();

            if (dependentTasks.Any())
            {
                var longestSubPath = new List<Domain.Entities.Task>();
                decimal maxSubDuration = 0;

                foreach (var dependentTask in dependentTasks)
                {
                    var subPath = GetLongestPath(dependentTask, taskDict, new HashSet<Guid>(visited));
                    var subDuration = subPath.Sum(t => t.EstimatedHours);
                    
                    if (subDuration > maxSubDuration)
                    {
                        maxSubDuration = subDuration;
                        longestSubPath = subPath;
                    }
                }

                path.AddRange(longestSubPath);
            }

            return path;
        }
    }
}

