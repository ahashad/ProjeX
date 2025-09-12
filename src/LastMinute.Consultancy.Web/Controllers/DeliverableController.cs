using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using LastMinute.Consultancy.Application.Deliverable;
using LastMinute.Consultancy.Application.Deliverable.Commands;
using LastMinute.Consultancy.Application.Project;

namespace LastMinute.Consultancy.Web.Controllers
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class DeliverableController : Controller
    {
        private readonly IDeliverableService _deliverableService;
        private readonly IProjectService _projectService;

        public DeliverableController(IDeliverableService deliverableService, IProjectService projectService)
        {
            _deliverableService = deliverableService;
            _projectService = projectService;
        }

        // GET: Deliverable
        public async Task<IActionResult> Index()
        {
            var deliverables = await _deliverableService.GetAllAsync();
            return View(deliverables);
        }

        // GET: Deliverable/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliverable = await _deliverableService.GetByIdAsync(id.Value);
            if (deliverable == null)
            {
                return NotFound();
            }

            return View(deliverable);
        }

        // GET: Deliverable/Create
        public async Task<IActionResult> Create()
        {
            await PopulateProjectsDropdown();
            return View();
        }

        // POST: Deliverable/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDeliverableCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _deliverableService.CreateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating deliverable: {ex.Message}");
                }
            }
            await PopulateProjectsDropdown(command.ProjectId);
            return View(command);
        }

        // GET: Deliverable/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliverable = await _deliverableService.GetByIdAsync(id.Value);
            if (deliverable == null)
            {
                return NotFound();
            }

            var command = new UpdateDeliverableCommand
            {
                Id = deliverable.Id,
                ProjectId = deliverable.ProjectId,
                Title = deliverable.Title,
                Description = deliverable.Description,
                DueDate = deliverable.DueDate,
                Status = deliverable.Status
            };

            await PopulateProjectsDropdown(command.ProjectId);
            return View(command);
        }

        // POST: Deliverable/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateDeliverableCommand command)
        {
            if (id != command.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _deliverableService.UpdateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating deliverable: {ex.Message}");
                }
            }
            await PopulateProjectsDropdown(command.ProjectId);
            return View(command);
        }

        // GET: Deliverable/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deliverable = await _deliverableService.GetByIdAsync(id.Value);
            if (deliverable == null)
            {
                return NotFound();
            }

            return View(deliverable);
        }

        // POST: Deliverable/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _deliverableService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting deliverable: {ex.Message}");
                var deliverable = await _deliverableService.GetByIdAsync(id);
                return View(deliverable);
            }
        }

        private async Task PopulateProjectsDropdown(Guid? selectedProjectId = null)
        {
            var projects = await _projectService.GetAllAsync();
            ViewBag.ProjectId = new SelectList(projects, "Id", "ProjectName", selectedProjectId);
        }
    }
}

