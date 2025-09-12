using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using LastMinute.Consultancy.Application.Overhead;
using LastMinute.Consultancy.Application.Overhead.Commands;
using LastMinute.Consultancy.Application.Project;

namespace LastMinute.Consultancy.Web.Controllers
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class OverheadController : Controller
    {
        private readonly IOverheadService _overheadService;
        private readonly IProjectService _projectService;
        public OverheadController(IOverheadService overheadService, IProjectService projectService)
        {
            _overheadService = overheadService;
            _projectService = projectService;
        }

        // GET: Overhead
        public async Task<IActionResult> Index()
        {
            var overheads = await _overheadService.GetAllAsync();
            return View(overheads);
        }

        // GET: Overhead/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var overhead = await _overheadService.GetByIdAsync(id.Value);
            if (overhead == null)
            {
                return NotFound();
            }

            return View(overhead);
        }

        // GET: Overhead/Create
        public async Task<IActionResult> Create()
        {
            await PopulateProjectsDropdown();
            return View();
        }

        // POST: Overhead/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOverheadCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _overheadService.CreateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating overhead: {ex.Message}");
                }
            }
            await PopulateProjectsDropdown(command.ProjectId);
            return View(command);
        }

        // GET: Overhead/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var overhead = await _overheadService.GetByIdAsync(id.Value);
            if (overhead == null)
            {
                return NotFound();
            }

            var command = new UpdateOverheadCommand
            {
                Id = overhead.Id,
                ProjectId = overhead.ProjectId,
                Description = overhead.Description,
                Category = overhead.Category,
                Amount = overhead.Amount,
                Date = overhead.Date
            };

            await PopulateProjectsDropdown(command.ProjectId);
            return View(command);
        }

        // POST: Overhead/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateOverheadCommand command)
        {
            if (id != command.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _overheadService.UpdateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating overhead: {ex.Message}");
                }
            }
            await PopulateProjectsDropdown(command.ProjectId);
            return View(command);
        }

        // GET: Overhead/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var overhead = await _overheadService.GetByIdAsync(id.Value);
            if (overhead == null)
            {
                return NotFound();
            }

            return View(overhead);
        }

        // POST: Overhead/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _overheadService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting overhead: {ex.Message}");
                var overhead = await _overheadService.GetByIdAsync(id);
                return View(overhead);
            }
        }

        private async Task PopulateProjectsDropdown(Guid? selectedProjectId = null)
        {
            var projects = await _projectService.GetAllAsync();
            ViewBag.ProjectId = new SelectList(projects, "Id", "ProjectName", selectedProjectId);
        }
    }
}

