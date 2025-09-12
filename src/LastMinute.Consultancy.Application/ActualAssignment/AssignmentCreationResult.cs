using System.Collections.Generic;
using LastMinute.Consultancy.Application.Employee;

namespace LastMinute.Consultancy.Application.ActualAssignment
{
    public class AssignmentCreationResult
    {
        public ActualAssignmentDto? Assignment { get; set; }
        public ActualAssignmentDto? ConflictingAssignment { get; set; }
        public List<EmployeeDto> AvailableEmployees { get; set; } = new();
    }
}
