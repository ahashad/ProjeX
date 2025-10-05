namespace ProjeX.Application.ActualAssignment
{
    public class AutoCompleteResult
    {
        public int CompletedCount { get; set; }
        public List<Guid> CompletedAssignmentIds { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public bool IsSuccessful { get; set; } = true;
        public TimeSpan Duration { get; set; }
    }
}