using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using LastMinute.Consultancy.Application.ActualAssignment;
using LastMinute.Consultancy.Application.ActualAssignment.Commands;
using LastMinute.Consultancy.Application.Project;
using LastMinute.Consultancy.Application.Employee;
using LastMinute.Consultancy.Application.PlannedTeamSlot;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Web.Controllers
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class ActualAssignmentController : Controller
    {
        private readonly IAssignmentService _assignmentService;
        private readonly IPlannedTeamSlotService _plannedTeamSlotService;
        private readonly IProjectService _projectService;
        private readonly IEmployeeService _employeeService;

        public ActualAssignmentController(IAssignmentService assignmentService, IPlannedTeamSlotService plannedTeamSlotService, IProjectService projectService, IEmployeeService employeeService)
        {
            _assignmentService = assignmentService;
            _plannedTeamSlotService = plannedTeamSlotService;
            _projectService = projectService;
            _employeeService = employeeService;
        }

        // GET: ActualAssignment
        public async Task<IActionResult> Index(Guid? projectId, Guid? employeeId)
        {
            var assignments = await _assignmentService.GetAssignmentsAsync(projectId, employeeId);

            ViewBag.ProjectId = projectId;
            ViewBag.EmployeeId = employeeId;
            
            return View(assignments);
        }

        // GET: ActualAssignment/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var assignment = await _assignmentService.GetByIdAsync(id.Value);
            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // GET: ActualAssignment/Create
        public async Task<IActionResult> Create(Guid? projectId, Guid? roleId, Guid? plannedTeamSlotId)
        {
            await PopulateDropdowns(projectId, plannedTeamSlotId, roleId);
            
            var model = new CreateActualAssignmentCommand();
            if (projectId.HasValue)
            {
                model.ProjectId = projectId.Value;
            }
            
            // If plannedTeamSlotId is provided, pre-populate it and get the slot details
            if (plannedTeamSlotId.HasValue)
            {
                model.PlannedTeamSlotId = plannedTeamSlotId.Value;
                
                // Get the planned team slot details to pre-populate allocation percentage
                var plannedSlot = await _plannedTeamSlotService.GetSlotByIdAsync(plannedTeamSlotId.Value);
                if (plannedSlot != null)
                {
                    model.AllocationPercent = plannedSlot.AllocationPercent;
                    ViewBag.PlannedSlotDetails = plannedSlot;
                }
            }
            else if (roleId.HasValue && projectId.HasValue)
            {
                // If only roleId is provided, try to auto-select the matching planned slot
                var availableSlots = await _plannedTeamSlotService.GetAvailableSlotsAsync(projectId.Value);
                var slotForRole = availableSlots.FirstOrDefault(s => s.RoleId == roleId.Value);
                if (slotForRole != null)
                {
                    model.PlannedTeamSlotId = slotForRole.Id;
                    model.AllocationPercent = slotForRole.AllocationPercent;
                    ViewBag.PlannedSlotDetails = slotForRole;
                }
            }
            
            return View(model);
        }

        // POST: ActualAssignment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateActualAssignmentCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var result = await _assignmentService.CreateAsync(command, User.Identity?.Name ?? "current-user");

                    if (result.Assignment != null)
                    {
                        TempData["Success"] = "Assignment created successfully and is pending approval.";
                        return RedirectToAction(nameof(Index));
                    }

                    ModelState.AddModelError("", "This planned team slot is already assigned for the selected dates.");
                    ViewBag.ConflictingAssignment = result.ConflictingAssignment;
                    ViewBag.AvailableEmployees = result.AvailableEmployees;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            await PopulateDropdowns(command.ProjectId, command.PlannedTeamSlotId);
            
            // Re-populate planned slot details if needed
            if (command.PlannedTeamSlotId != Guid.Empty)
            {
                var plannedSlot = await _plannedTeamSlotService.GetSlotByIdAsync(command.PlannedTeamSlotId);
                if (plannedSlot != null)
                {
                    ViewBag.PlannedSlotDetails = plannedSlot;
                }
            }
            
            return View(command);
        }

        // GET: ActualAssignment/Unassign/5
        public async Task<IActionResult> Unassign(Guid id)
        {
            var assignment = await _assignmentService.GetByIdAsync(id);
            if (assignment == null)
            {
                return NotFound();
            }

            var model = new UnassignActualAssignmentCommand
            {
                AssignmentId = id,
                EndDate = assignment.EndDate ?? assignment.ProjectEndDate
            };

            ViewBag.Assignment = assignment;
            return View(model);
        }

        // POST: ActualAssignment/Unassign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unassign(UnassignActualAssignmentCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _assignmentService.UnassignAsync(command, User.Identity?.Name ?? "current-user");
                    TempData["Success"] = "Assignment unassigned successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            var assignment = await _assignmentService.GetByIdAsync(command.AssignmentId);
            ViewBag.Assignment = assignment;
            return View(command);
        }

        // POST: ActualAssignment/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id)
        {
            try
            {
                await _assignmentService.ApproveAsync(id, User.Identity?.Name ?? "current-user");
                TempData["Success"] = "Assignment has been approved.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            
            return RedirectToAction(nameof(Index));
        }

        // POST: ActualAssignment/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id, string reason)
        {
            try
            {
                await _assignmentService.RejectAsync(id, User.Identity?.Name ?? "current-user", reason ?? "No reason provided");
                TempData["Success"] = "Assignment has been rejected.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(Guid? projectId = null, Guid? selectedPlannedTeamSlotId = null, Guid? roleId = null)
        {
            // Get active projects only
            var projects = await _projectService.GetAllAsync();
            var activeProjects = projects.Where(p => p.Status == Domain.Enums.ProjectStatus.InProgress).ToList();
            ViewBag.ProjectId = new SelectList(activeProjects, "Id", "ProjectName", projectId);

            // Get available planned team slots for the project
            if (projectId.HasValue)
            {
                var availableSlots = await _plannedTeamSlotService.GetAvailableSlotsAsync(projectId.Value);
                
                // Filter by roleId if provided for auto-selection
                Guid? autoSelectSlotId = selectedPlannedTeamSlotId;
                if (roleId.HasValue && !selectedPlannedTeamSlotId.HasValue)
                {
                    var slotForRole = availableSlots.FirstOrDefault(s => s.RoleId == roleId.Value);
                    if (slotForRole != null)
                    {
                        autoSelectSlotId = slotForRole.Id;
                    }
                }
                
                ViewBag.PlannedTeamSlotId = new SelectList(
                    availableSlots.Select(s => new { 
                        Id = s.Id, 
                        Description = $"{s.RoleName} - {s.PeriodMonths} months ({s.AllocationPercent}%)",
                        AllocationPercent = s.AllocationPercent,
                        PeriodMonths = s.PeriodMonths,
                        RoleId = s.RoleId
                    }), 
                    "Id", 
                    "Description",
                    autoSelectSlotId
                );
                
                // Store the auto-selected slot ID for JavaScript
                ViewBag.AutoSelectedSlotId = autoSelectSlotId;
            }
            else
            {
                ViewBag.PlannedTeamSlotId = new SelectList(new List<object>(), "Id", "Description");
                ViewBag.AutoSelectedSlotId = null;
            }

            // Get employees with detailed information for enhanced dropdown
            var employees = await _employeeService.GetAllAsync();
            var activeEmployees = employees.Where(e => e.IsActive).ToList();
            
            // Create enhanced employee options with name, role, salary, and incentive
            ViewBag.EmployeeId = new SelectList(
                activeEmployees.Select(e => new { 
                    Id = e.Id, 
                    Display = $"{e.FullName} - {e.RoleName} | Salary: ${e.Salary:N0} | Incentive: ${e.MonthlyIncentive:N0}",
                    FullName = e.FullName,
                    RoleName = e.RoleName,
                    RoleId = e.RoleId,
                    Salary = e.Salary,
                    MonthlyIncentive = e.MonthlyIncentive,
                    Email = e.Email
                }), 
                "Id", 
                "Display"
            );
        }
        
        // AJAX endpoint to get available planned positions for a project
        [HttpGet]
        public async Task<IActionResult> GetAvailablePlannedPositions(Guid projectId, Guid? roleId = null)
        {
            try
            {
                var availableSlots = await _plannedTeamSlotService.GetAvailableSlotsAsync(projectId);
                
                var positions = availableSlots.Select(s => new { 
                    Id = s.Id, 
                    Description = $"{s.RoleName} - {s.PeriodMonths} months ({s.AllocationPercent}%)",
                    AllocationPercent = s.AllocationPercent,
                    PeriodMonths = s.PeriodMonths,
                    RoleName = s.RoleName,
                    RoleId = s.RoleId
                }).ToList();
                
                // Auto-select position by roleId if provided
                var autoSelectId = Guid.Empty;
                if (roleId.HasValue)
                {
                    var matchingPosition = positions.FirstOrDefault(p => p.RoleId == roleId.Value);
                    if (matchingPosition != null)
                    {
                        autoSelectId = matchingPosition.Id;
                    }
                }
                
                return Json(new 
                {
                    positions = positions,
                    autoSelectId = autoSelectId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        // AJAX endpoint to get planned slot details
        [HttpGet]
        public async Task<IActionResult> GetPlannedSlotDetails(Guid plannedTeamSlotId)
        {
            try
            {
                var plannedSlot = await _plannedTeamSlotService.GetSlotByIdAsync(plannedTeamSlotId);
                if (plannedSlot == null)
                {
                    return NotFound();
                }
                
                return Json(new 
                {
                    allocationPercent = plannedSlot.AllocationPercent,
                    periodMonths = plannedSlot.PeriodMonths,
                    roleName = plannedSlot.RoleName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        // AJAX endpoint to get employees with detailed information for search
        [HttpGet]
        public async Task<IActionResult> GetEmployeesForAssignment(Guid? roleId = null, string search = null, int page = 1)
        {
            try
            {
                var employees = await _employeeService.GetAllAsync();
                var activeEmployees = employees.Where(e => e.IsActive).ToList();
     
                // Filter by role if provided
                if (roleId.HasValue)
                {
                    activeEmployees = activeEmployees.Where(e => e.RoleId == roleId.Value).ToList();
                }

                // Apply search filter if search term is provided
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var searchTerm = search.ToLower().Trim();
                    activeEmployees = activeEmployees.Where(e => 
                       e.FullName.ToLower().Contains(searchTerm) ||
 e.RoleName.ToLower().Contains(searchTerm) ||
      e.Email.ToLower().Contains(searchTerm)
                     ).ToList();
                }
     
                var employeeOptions = activeEmployees.Select(e => new { 
                    id = e.Id,
                    text = $"{e.FullName} - {e.RoleName}",
                    fullName = e.FullName,
                    roleName = e.RoleName,
                    roleId = e.RoleId,
                    salary = e.Salary,
                    monthlyIncentive = e.MonthlyIncentive,
                    email = e.Email,
                    displayText = $"{e.FullName} - {e.RoleName} | ${e.Salary:N0} salary + ${e.MonthlyIncentive:N0} incentive",
                    searchTerms = $"{e.FullName} {e.RoleName} {e.Email}".ToLower()
                }).ToList();
      
                return Json(new 
                {
                    employees = employeeOptions,
                    count = employeeOptions.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

