using AutoMapper;
using LastMinute.Consultancy.Application.TimeEntry.Commands;
using LastMinute.Consultancy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Application.TimeEntry
{
  public class TimeEntryService : ITimeEntryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TimeEntryService(ApplicationDbContext context, IMapper mapper)
        {
   _context = context;
   _mapper = mapper;
        }

    public async Task<TimeEntryDto> CreateAsync(CreateTimeEntryCommand command, string userId)
        {
   // Validate actual assignment exists and is active
         var actualAssignment = await _context.ActualAssignments
       .Include(aa => aa.Project)
   .Include(aa => aa.Employee)
     .Include(aa => aa.PlannedTeamSlot)
          .ThenInclude(pts => pts.Role)
      .FirstOrDefaultAsync(aa => aa.Id == command.ActualAssignmentId && !aa.IsDeleted);

      if (actualAssignment == null)
            {
      throw new InvalidOperationException("Assignment not found");
            }

if (actualAssignment.Status != AssignmentStatus.Active)
      {
        throw new InvalidOperationException("Can only log time for active assignments");
       }

      // Validate date is not in the future
            if (command.Date > DateTime.Today)
      {
 throw new InvalidOperationException("Cannot log time for future dates");
          }

            // Validate no overlapping time entries for the same assignment on the same date
    var existingEntry = await _context.TimeEntries
    .FirstOrDefaultAsync(te => te.ActualAssignmentId == command.ActualAssignmentId && 
      te.Date.Date == command.Date.Date && 
          !te.IsDeleted);

        if (existingEntry != null)
    {
          throw new InvalidOperationException("A time entry already exists for this assignment on this date. Please edit the existing entry.");
     }

       // Set billable rate if not provided
            decimal? billableRate = command.BillableRate;
       if (command.IsBillable && !billableRate.HasValue)
            {
      // Use employee's hourly rate based on salary (assuming monthly salary / 160 hours)
 var monthlySalary = actualAssignment.Employee.Salary + actualAssignment.Employee.MonthlyIncentive;
       billableRate = Math.Round(monthlySalary / 160m, 2);
            }

            var timeEntry = new Domain.Entities.TimeEntry
            {
       Id = Guid.NewGuid(),
         ActualAssignmentId = command.ActualAssignmentId,
                Date = command.Date,
   Hours = command.Hours,
       Description = command.Description,
          Status = TimeEntryStatus.Draft,
 IsBillable = command.IsBillable,
     BillableRate = billableRate,
    CreatedBy = userId,
  CreatedAt = DateTime.UtcNow,
       ModifiedBy = userId,
     ModifiedAt = DateTime.UtcNow
            };

            _context.TimeEntries.Add(timeEntry);
         await _context.SaveChangesAsync();

            // Reload with includes
        var savedTimeEntry = await _context.TimeEntries
         .Include(te => te.ActualAssignment)
    .ThenInclude(aa => aa.Project)
                .ThenInclude(p => p.Client)
      .Include(te => te.ActualAssignment)
   .ThenInclude(aa => aa.Employee)
    .Include(te => te.ActualAssignment)
        .ThenInclude(aa => aa.PlannedTeamSlot)
        .ThenInclude(pts => pts.Role)
         .FirstAsync(te => te.Id == timeEntry.Id);

       return _mapper.Map<TimeEntryDto>(savedTimeEntry);
     }

      public async Task<TimeEntryDto> UpdateAsync(UpdateTimeEntryCommand command, string userId)
    {
            var timeEntry = await _context.TimeEntries
            .Include(te => te.ActualAssignment)
     .ThenInclude(aa => aa.Employee)
      .FirstOrDefaultAsync(te => te.Id == command.Id && !te.IsDeleted);

    if (timeEntry == null)
       {
       throw new InvalidOperationException("Time entry not found");
            }

   // Only allow editing draft entries or by admin/manager
        if (timeEntry.Status != TimeEntryStatus.Draft && !IsManagerOrAdmin(userId))
         {
      throw new InvalidOperationException("Only draft time entries can be edited");
 }

 // Validate date is not in the future
            if (command.Date > DateTime.Today)
         {
throw new InvalidOperationException("Cannot log time for future dates");
        }

            // Check for overlapping entries (excluding current entry)
       var overlappingEntry = await _context.TimeEntries
       .FirstOrDefaultAsync(te => te.ActualAssignmentId == command.ActualAssignmentId && 
  te.Date.Date == command.Date.Date && 
   te.Id != command.Id && 
  !te.IsDeleted);

            if (overlappingEntry != null)
 {
     throw new InvalidOperationException("Another time entry exists for this assignment on this date");
     }

         // Set billable rate if not provided and is billable
            decimal? billableRate = command.BillableRate;
       if (command.IsBillable && !billableRate.HasValue && !timeEntry.BillableRate.HasValue)
            {
     var monthlySalary = timeEntry.ActualAssignment.Employee.Salary + timeEntry.ActualAssignment.Employee.MonthlyIncentive;
    billableRate = Math.Round(monthlySalary / 160m, 2);
         }

    timeEntry.ActualAssignmentId = command.ActualAssignmentId;
      timeEntry.Date = command.Date;
   timeEntry.Hours = command.Hours;
 timeEntry.Description = command.Description;
            timeEntry.Status = command.Status;
 timeEntry.IsBillable = command.IsBillable;
            timeEntry.BillableRate = command.IsBillable ? (command.BillableRate ?? timeEntry.BillableRate ?? billableRate) : null;
     timeEntry.ModifiedBy = userId;
      timeEntry.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

// Reload with includes
       var updatedTimeEntry = await _context.TimeEntries
.Include(te => te.ActualAssignment)
                .ThenInclude(aa => aa.Project)
       .ThenInclude(p => p.Client)
            .Include(te => te.ActualAssignment)
          .ThenInclude(aa => aa.Employee)
          .Include(te => te.ActualAssignment)
     .ThenInclude(aa => aa.PlannedTeamSlot)
                .ThenInclude(pts => pts.Role)
     .FirstAsync(te => te.Id == timeEntry.Id);

       return _mapper.Map<TimeEntryDto>(updatedTimeEntry);
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
     var timeEntry = await _context.TimeEntries
           .FirstOrDefaultAsync(te => te.Id == id && !te.IsDeleted);

            if (timeEntry == null)
       {
   throw new InvalidOperationException("Time entry not found");
        }

            // Only allow deleting draft entries or by admin/manager
            if (timeEntry.Status != TimeEntryStatus.Draft && !IsManagerOrAdmin(userId))
            {
       throw new InvalidOperationException("Only draft time entries can be deleted");
  }

          timeEntry.IsDeleted = true;
            timeEntry.ModifiedBy = userId;
          timeEntry.ModifiedAt = DateTime.UtcNow;

     await _context.SaveChangesAsync();
        }

        public async Task<List<TimeEntryDto>> GetTimeEntriesAsync(Guid? actualAssignmentId, Guid? employeeId, DateTime? fromDate = null, DateTime? toDate = null)
        {
    var query = _context.TimeEntries
                .Include(te => te.ActualAssignment)
          .ThenInclude(aa => aa.Project)
        .ThenInclude(p => p.Client)
       .Include(te => te.ActualAssignment)
          .ThenInclude(aa => aa.Employee)
 .Include(te => te.ActualAssignment)
        .ThenInclude(aa => aa.PlannedTeamSlot)
    .ThenInclude(pts => pts.Role)
  .Where(te => !te.IsDeleted);

  if (actualAssignmentId.HasValue)
       {
  query = query.Where(te => te.ActualAssignmentId == actualAssignmentId.Value);
    }

            if (employeeId.HasValue)
            {
            query = query.Where(te => te.ActualAssignment.EmployeeId == employeeId.Value);
   }

   if (fromDate.HasValue)
   {
             query = query.Where(te => te.Date >= fromDate.Value);
  }

        if (toDate.HasValue)
        {
      query = query.Where(te => te.Date <= toDate.Value);
    }

          var timeEntries = await query
                .OrderByDescending(te => te.Date)
       .ThenBy(te => te.CreatedAt)
      .ToListAsync();

   return _mapper.Map<List<TimeEntryDto>>(timeEntries);
        }

        public async Task<TimeEntryDto?> GetByIdAsync(Guid id)
      {
     var timeEntry = await _context.TimeEntries
        .Include(te => te.ActualAssignment)
                .ThenInclude(aa => aa.Project)
    .ThenInclude(p => p.Client)
        .Include(te => te.ActualAssignment)
     .ThenInclude(aa => aa.Employee)
        .Include(te => te.ActualAssignment)
     .ThenInclude(aa => aa.PlannedTeamSlot)
                .ThenInclude(pts => pts.Role)
      .FirstOrDefaultAsync(te => te.Id == id && !te.IsDeleted);

        return timeEntry != null ? _mapper.Map<TimeEntryDto>(timeEntry) : null;
        }

        public async Task SubmitAsync(Guid id, string userId)
   {
var timeEntry = await _context.TimeEntries
      .FirstOrDefaultAsync(te => te.Id == id && !te.IsDeleted);

            if (timeEntry == null)
            {
      throw new InvalidOperationException("Time entry not found");
    }

            if (timeEntry.Status != TimeEntryStatus.Draft)
   {
      throw new InvalidOperationException("Only draft time entries can be submitted");
            }

            timeEntry.Status = TimeEntryStatus.Submitted;
     timeEntry.ModifiedBy = userId;
         timeEntry.ModifiedAt = DateTime.UtcNow;

   await _context.SaveChangesAsync();
    }

        public async Task ApproveAsync(Guid id, string approverUserId)
{
            var timeEntry = await _context.TimeEntries
             .FirstOrDefaultAsync(te => te.Id == id && !te.IsDeleted);

            if (timeEntry == null)
            {
    throw new InvalidOperationException("Time entry not found");
  }

   if (timeEntry.Status != TimeEntryStatus.Submitted)
  {
                throw new InvalidOperationException("Only submitted time entries can be approved");
            }

          timeEntry.Status = TimeEntryStatus.Approved;
            timeEntry.ApprovedBy = approverUserId;
            timeEntry.ApprovedAt = DateTime.UtcNow;
      timeEntry.ModifiedBy = approverUserId;
            timeEntry.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task RejectAsync(Guid id, string approverUserId, string reason)
      {
         var timeEntry = await _context.TimeEntries
          .FirstOrDefaultAsync(te => te.Id == id && !te.IsDeleted);

        if (timeEntry == null)
            {
       throw new InvalidOperationException("Time entry not found");
  }

            if (timeEntry.Status != TimeEntryStatus.Submitted)
          {
   throw new InvalidOperationException("Only submitted time entries can be rejected");
            }

            timeEntry.Status = TimeEntryStatus.Rejected;
            timeEntry.ApprovedBy = approverUserId;
     timeEntry.ApprovedAt = DateTime.UtcNow;
        timeEntry.Description = $"{timeEntry.Description}\n\nREJECTED: {reason}".Trim();
  timeEntry.ModifiedBy = approverUserId;
          timeEntry.ModifiedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
   }

        public async Task<List<TimeEntryDto>> GetForApprovalAsync()
        {
    var timeEntries = await _context.TimeEntries
            .Include(te => te.ActualAssignment)
           .ThenInclude(aa => aa.Project)
                .ThenInclude(p => p.Client)
           .Include(te => te.ActualAssignment)
       .ThenInclude(aa => aa.Employee)
     .Include(te => te.ActualAssignment)
                .ThenInclude(aa => aa.PlannedTeamSlot)
       .ThenInclude(pts => pts.Role)
   .Where(te => !te.IsDeleted && te.Status == TimeEntryStatus.Submitted)
    .OrderBy(te => te.Date)
        .ToListAsync();

            return _mapper.Map<List<TimeEntryDto>>(timeEntries);
        }

 public async Task<decimal> GetEmployeeHoursForPeriodAsync(Guid employeeId, DateTime fromDate, DateTime toDate)
        {
            return await _context.TimeEntries
    .Where(te => !te.IsDeleted && 
                 te.ActualAssignment.EmployeeId == employeeId &&
     te.Date >= fromDate && 
                 te.Date <= toDate &&
        (te.Status == TimeEntryStatus.Approved || te.Status == TimeEntryStatus.Invoiced))
   .SumAsync(te => te.Hours);
        }

        private static bool IsManagerOrAdmin(string userId)
  {
            // This would typically check user roles/claims
  // For now, returning false - implement according to your auth system
       return false;
    }
    }
}