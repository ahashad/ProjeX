using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using LastMinute.Consultancy.Application.TimeEntry;
using LastMinute.Consultancy.Application.TimeEntry.Commands;
using LastMinute.Consultancy.Application.ActualAssignment;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Web.Controllers
{
    [Authorize]
    public class TimeEntryController : Controller
    {
        private readonly ITimeEntryService _timeEntryService;
        private readonly IAssignmentService _assignmentService;

    public TimeEntryController(ITimeEntryService timeEntryService, IAssignmentService assignmentService)
   {
       _timeEntryService = timeEntryService;
            _assignmentService = assignmentService;
      }

        // GET: TimeEntry
        public async Task<IActionResult> Index(Guid? actualAssignmentId, Guid? employeeId, DateTime? fromDate, DateTime? toDate)
        {
     var timeEntries = await _timeEntryService.GetTimeEntriesAsync(actualAssignmentId, employeeId, fromDate, toDate);

            ViewBag.ActualAssignmentId = actualAssignmentId;
            ViewBag.EmployeeId = employeeId;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
          ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
       
            return View(timeEntries);
        }

        // GET: TimeEntry/Details/5
        public async Task<IActionResult> Details(Guid? id)
     {
            if (id == null)
   {
         return NotFound();
    }

            var timeEntry = await _timeEntryService.GetByIdAsync(id.Value);
            if (timeEntry == null)
            {
      return NotFound();
      }

      return View(timeEntry);
     }

        // GET: TimeEntry/Create
        public async Task<IActionResult> Create(Guid? actualAssignmentId)
{
         await PopulateDropdowns(actualAssignmentId);
            
     var model = new CreateTimeEntryCommand();
            if (actualAssignmentId.HasValue)
            {
           model.ActualAssignmentId = actualAssignmentId.Value;
       }
   
            return View(model);
   }

        // POST: TimeEntry/Create
    [HttpPost]
        [ValidateAntiForgeryToken]
      public async Task<IActionResult> Create(CreateTimeEntryCommand command)
        {
  if (ModelState.IsValid)
     {
    try
       {
  await _timeEntryService.CreateAsync(command, User.Identity?.Name ?? "current-user");
 TempData["Success"] = "Time entry created successfully.";
      return RedirectToAction(nameof(Index), new { actualAssignmentId = command.ActualAssignmentId });
        }
  catch (Exception ex)
 {
    ModelState.AddModelError("", ex.Message);
     }
          }

     await PopulateDropdowns(command.ActualAssignmentId);
         return View(command);
        }

        // GET: TimeEntry/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
    if (id == null)
            {
         return NotFound();
      }

            var timeEntry = await _timeEntryService.GetByIdAsync(id.Value);
            if (timeEntry == null)
            {
   return NotFound();
          }

  var command = new UpdateTimeEntryCommand
            {
 Id = timeEntry.Id,
             ActualAssignmentId = timeEntry.ActualAssignmentId,
            Date = timeEntry.Date,
    Hours = timeEntry.Hours,
       Description = timeEntry.Description,
         Status = timeEntry.Status,
 IsBillable = timeEntry.IsBillable,
                BillableRate = timeEntry.BillableRate
            };

            await PopulateDropdowns(timeEntry.ActualAssignmentId);
     ViewBag.TimeEntryDetails = timeEntry;
  
        return View(command);
    }

        // POST: TimeEntry/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateTimeEntryCommand command)
        {
     if (ModelState.IsValid)
          {
                try
         {
await _timeEntryService.UpdateAsync(command, User.Identity?.Name ?? "current-user");
     TempData["Success"] = "Time entry updated successfully.";
    return RedirectToAction(nameof(Index), new { actualAssignmentId = command.ActualAssignmentId });
            }
                catch (Exception ex)
                {
     ModelState.AddModelError("", ex.Message);
     }
       }

     await PopulateDropdowns(command.ActualAssignmentId);
            // Get time entry details for display
var timeEntry = await _timeEntryService.GetByIdAsync(command.Id);
            ViewBag.TimeEntryDetails = timeEntry;
  
            return View(command);
        }

        // POST: TimeEntry/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid? actualAssignmentId)
     {
  try
 {
 await _timeEntryService.DeleteAsync(id, User.Identity?.Name ?? "current-user");
  TempData["Success"] = "Time entry deleted successfully.";
            }
            catch (Exception ex)
            {
       TempData["Error"] = ex.Message;
   }
     
            return RedirectToAction(nameof(Index), new { actualAssignmentId });
      }

   // POST: TimeEntry/Submit/5
     [HttpPost]
[ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Guid id, Guid? actualAssignmentId)
        {
    try
  {
           await _timeEntryService.SubmitAsync(id, User.Identity?.Name ?? "current-user");
        TempData["Success"] = "Time entry submitted for approval.";
       }
      catch (Exception ex)
     {
     TempData["Error"] = ex.Message;
         }
     
    return RedirectToAction(nameof(Index), new { actualAssignmentId });
        }

 // GET: TimeEntry/Approval
        [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Approval()
    {
      var timeEntries = await _timeEntryService.GetForApprovalAsync();
            return View(timeEntries);
    }

        // POST: TimeEntry/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "ManagerOrAdmin")]
   public async Task<IActionResult> Approve(Guid id)
        {
       try
          {
     await _timeEntryService.ApproveAsync(id, User.Identity?.Name ?? "current-user");
      TempData["Success"] = "Time entry has been approved.";
   }
      catch (Exception ex)
        {
     TempData["Error"] = ex.Message;
            }
 
            return RedirectToAction(nameof(Approval));
      }

        // POST: TimeEntry/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
 [Authorize(Policy = "ManagerOrAdmin")]
        public async Task<IActionResult> Reject(Guid id, string reason)
        {
     try
    {
          await _timeEntryService.RejectAsync(id, User.Identity?.Name ?? "current-user", reason ?? "No reason provided");
      TempData["Success"] = "Time entry has been rejected.";
        }
          catch (Exception ex)
            {
       TempData["Error"] = ex.Message;
      }

    return RedirectToAction(nameof(Approval));
        }

private async Task PopulateDropdowns(Guid? selectedAssignmentId = null)
        {
   // Get all active assignments for dropdown
      var assignments = await _assignmentService.GetAssignmentsAsync(null, null);
  var activeAssignments = assignments.Where(a => a.Status == AssignmentStatus.Active).ToList();
            
      ViewBag.ActualAssignmentId = new SelectList(
                activeAssignments.Select(a => new { 
         Id = a.Id, 
         Description = $"{a.ProjectName} - {a.EmployeeName} ({a.RoleName})"
        }), 
             "Id", 
   "Description", 
          selectedAssignmentId
     );

 // Status options for edit
          ViewBag.StatusOptions = new SelectList(Enum.GetValues(typeof(TimeEntryStatus))
              .Cast<TimeEntryStatus>()
    .Select(s => new { Value = (int)s, Text = s.ToString() }), 
      "Value", "Text");
        }
    }
}