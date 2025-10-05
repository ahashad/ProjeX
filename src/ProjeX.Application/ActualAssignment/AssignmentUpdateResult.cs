namespace ProjeX.Application.ActualAssignment
{
    public class AssignmentUpdateResult
    {
        public ActualAssignmentDto? Assignment { get; set; }
        public bool IsSuccessful { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}