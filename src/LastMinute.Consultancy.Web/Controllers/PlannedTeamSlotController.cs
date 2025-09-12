using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using LastMinute.Consultancy.Application.PlannedTeamSlot;
using LastMinute.Consultancy.Application.PlannedTeamSlot.Commands;
using LastMinute.Consultancy.Application.Project;
using LastMinute.Consultancy.Application.RolesCatalog;

namespace LastMinute.Consultancy.Web.Controllers
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class PlannedTeamSlotController : Controller
    {
        private readonly IPlannedTeamSlotService _plannedTeamSlotService;
        private readonly IProjectService _projectService;
        private readonly IRolesCatalogService _rolesCatalogService;

        public PlannedTeamSlotController(IPlannedTeamSlotService plannedTeamSlotService, IProjectService projectService, IRolesCatalogService rolesCatalogService)
        {
            _plannedTeamSlotService = plannedTeamSlotService;
            _projectService = projectService;
            _rolesCatalogService = rolesCatalogService;
        }

        // GET: PlannedTeamSlot
        public async Task<IActionResult> Index(Guid? projectId)
        {
            ViewBag.ProjectId = projectId;
            
            if (projectId.HasValue)
            {
                var slots = await _plannedTeamSlotService.GetSlotsByProjectAsync(projectId.Value);

                // Calculate total budget cost
                ViewBag.TotalBudgetCost = slots.Sum(s => s.ComputedBudgetCost);

                return View(slots);
            }

            return View(new List<PlannedTeamSlotDto>());
        }

        // GET: PlannedTeamSlot/Create
        public async Task<IActionResult> Create(Guid? projectId)
        {
            await PopulateDropdowns();
            
            var model = new CreatePlannedTeamSlotCommand();
            if (projectId.HasValue)
            {
                model.ProjectId = projectId.Value;
            }
            
            return View(model);
        }

        // POST: PlannedTeamSlot/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePlannedTeamSlotCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.Identity?.Name ?? "current-user"; // TODO: Get proper user ID
                    await _plannedTeamSlotService.CreateSlotAsync(command, userId);
                    TempData["Success"] = "Planned team slot created successfully.";
                    return RedirectToAction(nameof(Index), new { projectId = command.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            await PopulateDropdowns();
            return View(command);
        }

        // GET: PlannedTeamSlot/Edit/5
        public async Task<IActionResult> Edit(Guid id)
        {
            var slot = await _plannedTeamSlotService.GetSlotByIdAsync(id);
            
            if (slot == null)
            {
                return NotFound();
            }

            var command = new UpdatePlannedTeamSlotCommand
            {
                Id = slot.Id,
                ProjectId = slot.ProjectId,
                RoleId = slot.RoleId,
                PeriodMonths = slot.PeriodMonths,
                AllocationPercent = slot.AllocationPercent,
                PlannedSalary = slot.PlannedSalary,
                PlannedIncentive = slot.PlannedIncentive,
                PlannedCommissionPercent = slot.PlannedCommissionPercent,
                RowVersion = new byte[0] // TODO: Get proper row version from entity
            };

            await PopulateDropdowns();
            ViewBag.RemainingAllocationPercent = slot.RemainingAllocationPercent;
            return View(command);
        }

        // POST: PlannedTeamSlot/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdatePlannedTeamSlotCommand command)
        {
            if (id != command.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.Identity?.Name ?? "current-user"; // TODO: Get proper user ID
                    await _plannedTeamSlotService.UpdateSlotAsync(command, userId);
                    TempData["Success"] = "Planned team slot updated successfully.";
                    return RedirectToAction(nameof(Index), new { projectId = command.ProjectId });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            await PopulateDropdowns();
            return View(command);
        }

        // POST: PlannedTeamSlot/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, Guid projectId)
        {
            try
            {
                var userId = User.Identity?.Name ?? "current-user"; // TODO: Get proper user ID
                await _plannedTeamSlotService.DeleteSlotAsync(id, userId);
                TempData["Success"] = "Planned team slot deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index), new { projectId });
        }

        private async Task PopulateDropdowns()
        {
            var projects = await _projectService.GetAllAsync();
            ViewBag.ProjectId = new SelectList(projects, "Id", "ProjectName");

            var roles = await _rolesCatalogService.GetAllAsync();
            ViewBag.RoleId = new SelectList(roles, "Id", "RoleName");
        }
    }
}