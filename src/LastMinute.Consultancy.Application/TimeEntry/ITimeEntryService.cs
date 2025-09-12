using LastMinute.Consultancy.Application.TimeEntry.Commands;

namespace LastMinute.Consultancy.Application.TimeEntry
{
    public interface ITimeEntryService
    {
     Task<TimeEntryDto> CreateAsync(CreateTimeEntryCommand command, string userId);
     Task<TimeEntryDto> UpdateAsync(UpdateTimeEntryCommand command, string userId);
        Task DeleteAsync(Guid id, string userId);
        Task<List<TimeEntryDto>> GetTimeEntriesAsync(Guid? actualAssignmentId, Guid? employeeId, DateTime? fromDate = null, DateTime? toDate = null);
        Task<TimeEntryDto?> GetByIdAsync(Guid id);
Task SubmitAsync(Guid id, string userId);
      Task ApproveAsync(Guid id, string approverUserId);
       Task RejectAsync(Guid id, string approverUserId, string reason);
        Task<List<TimeEntryDto>> GetForApprovalAsync();
        Task<decimal> GetEmployeeHoursForPeriodAsync(Guid employeeId, DateTime fromDate, DateTime toDate);
    }
}