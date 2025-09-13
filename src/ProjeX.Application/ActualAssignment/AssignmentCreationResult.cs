using System.Collections.Generic;
using ProjeX.Application.Employee;

namespace ProjeX.Application.ActualAssignment
{
    public class AssignmentCreationResult
    {
        public ActualAssignmentDto? Assignment { get; set; }
        public ActualAssignmentDto? ConflictingAssignment { get; set; }
        public List<EmployeeDto> AvailableEmployees { get; set; } = new();
    }
}
