using System.Collections.Generic;
using ProjeX.Application.Employee;

namespace ProjeX.Application.ActualAssignment
{
    public class AssignmentCreationResult
    {
        public ActualAssignmentDto? Assignment { get; set; }
        public ActualAssignmentDto? ConflictingAssignment { get; set; }
        public List<EmployeeDto> AvailableEmployees { get; set; } = new();
        public bool IsSuccessful { get; set; } = true;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
