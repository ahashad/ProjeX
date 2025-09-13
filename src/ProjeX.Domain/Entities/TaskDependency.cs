namespace ProjeX.Domain.Entities
{
    public class TaskDependency
    {
        public Guid TaskId { get; set; }
        public Task Task { get; set; } = null!;
        
        public Guid DependentTaskId { get; set; }
        public Task DependentTask { get; set; } = null!;
    }
}

