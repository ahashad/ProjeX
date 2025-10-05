using ProjeX.Application.ActualAssignment.Commands;

namespace ProjeX.Application.ActualAssignment
{
    public interface IAssignmentService
    {
        Task<AssignmentCreationResult> CreateAsync(CreateActualAssignmentCommand command, string userId);
        Task<AssignmentCreationResult> ValidateAssignmentAsync(CreateActualAssignmentCommand command);
        Task<AssignmentValidationResult> ValidateAssignmentRealtimeAsync(CreateActualAssignmentCommand command);
        Task<AssignmentUpdateResult> UpdateAsync(UpdateActualAssignmentCommand command, string userId);
        Task DeleteAsync(Guid assignmentId, string userId);
        Task ApproveAsync(Guid assignmentId, string approverUserId);
        Task RejectAsync(Guid assignmentId, string approverUserId, string reason);
        Task DeclineAsync(Guid assignmentId, string approverUserId, string reason);
        Task UnassignAsync(UnassignActualAssignmentCommand command, string userId);
        Task<List<ActualAssignmentDto>> GetAssignmentsAsync(Guid? projectId, Guid? employeeId);
        Task<ActualAssignmentDto?> GetByIdAsync(Guid id);
        Task<decimal> GetEmployeeAllocationAsync(Guid employeeId, DateTime startDate, DateTime endDate);
        Task<List<EmployeeUtilizationPointDto>> GetEmployeeUtilizationAsync(Guid employeeId, DateTime from, DateTime to);
        Task<List<ActualAssignmentDto>> GetAssignmentsBySlotAsync(Guid plannedTeamSlotId, Guid roleId);
        Task<AutoCompleteResult> AutoCompleteAssignmentsAsync(string userId);
        Task<DateTime?> GetMostRecentCompletedAssignmentEndDateAsync(Guid projectId, Guid plannedTeamSlotId);
    }
}