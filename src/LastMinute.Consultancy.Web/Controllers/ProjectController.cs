using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using LastMinute.Consultancy.Application.Project;
using LastMinute.Consultancy.Application.Project.Commands;
using LastMinute.Consultancy.Application.Client;

namespace LastMinute.Consultancy.Web.Controllers
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class ProjectController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly IClientService _clientService;

        public ProjectController(IProjectService projectService, IClientService clientService)
        {
            _projectService = projectService;
            _clientService = clientService;
        }

        // GET: Project
        public async Task<IActionResult> Index()
        {
            var projects = await _projectService.GetAllAsync();
            return View(projects);
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetByIdAsync(id.Value);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Project/Create
        public async Task<IActionResult> Create()
        {
            await PopulateClientsDropdown();
            return View();
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProjectCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _projectService.CreateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating project: {ex.Message}");
                }
            }
            await PopulateClientsDropdown(command.ClientId);
            return View(command);
        }

        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetByIdAsync(id.Value);
            if (project == null)
            {
                return NotFound();
            }

            var command = new UpdateProjectCommand
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                ClientId = project.ClientId,
                StartDate = project.StartDate,
                EndDate = project.EndDate,
                Budget = project.Budget,
                ProjectPrice = project.ProjectPrice,
                ExpectedWorkingPeriodMonths = project.ExpectedWorkingPeriodMonths,
                Status = project.Status,
                Notes = project.Notes
            };

            await PopulateClientsDropdown(command.ClientId);
            return View(command);
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateProjectCommand command)
        {
            if (id != command.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _projectService.UpdateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating project: {ex.Message}");
                }
            }
            await PopulateClientsDropdown(command.ClientId);
            return View(command);
        }

        // GET: Project/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var project = await _projectService.GetByIdAsync(id.Value);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _projectService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting project: {ex.Message}");
                var project = await _projectService.GetByIdAsync(id);
                return View(project);
            }
        }

        // GET: Project/Approve/5
        public async Task<IActionResult> Approve(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var projectToApprove = await _projectService.GetByIdAsync(id.Value);

            if (projectToApprove == null)
            {
                return NotFound();
            }

            if (projectToApprove.Status != Domain.Enums.ProjectStatus.Planned)
            {
                TempData["Error"] = "Only planned projects can be approved.";
                return RedirectToAction(nameof(Index));
            }

            return View(projectToApprove);
        }

        // POST: Project/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(Guid id, DateTime startDate, DateTime endDate, string approvalNotes)
        {
            try
            {
                var command = new ApproveProjectCommand
                {
                    ProjectId = id,
                    StartDate = startDate,
                    EndDate = endDate,
                    ApprovalNotes = approvalNotes ?? string.Empty,
                    ApprovedDate = DateTime.UtcNow
                };

                await _projectService.ApproveAsync(command);
                TempData["Success"] = "Project has been approved and set to active status.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task PopulateClientsDropdown(Guid? selectedClientId = null)
        {
            var clients = await _clientService.GetAllAsync();
            ViewBag.ClientId = new SelectList(clients, "Id", "ClientName", selectedClientId);
        }
    }
}

