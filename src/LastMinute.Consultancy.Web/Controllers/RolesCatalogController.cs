using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using LastMinute.Consultancy.Application.RolesCatalog;
using LastMinute.Consultancy.Application.RolesCatalog.Commands;

namespace LastMinute.Consultancy.Web.Controllers
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class RolesCatalogController : Controller
    {
        private readonly IRolesCatalogService _rolesCatalogService;

        public RolesCatalogController(IRolesCatalogService rolesCatalogService)
        {
            _rolesCatalogService = rolesCatalogService;
        }

        // GET: RolesCatalog
        public async Task<IActionResult> Index()
        {
            var roles = await _rolesCatalogService.GetAllAsync();
            return View(roles);
        }

        // GET: RolesCatalog/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _rolesCatalogService.GetByIdAsync(id.Value);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // GET: RolesCatalog/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RolesCatalog/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRolesCatalogCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _rolesCatalogService.CreateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating role: {ex.Message}");
                }
            }
            return View(command);
        }

        // GET: RolesCatalog/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _rolesCatalogService.GetByIdAsync(id.Value);
            if (role == null)
            {
                return NotFound();
            }

            var command = new UpdateRolesCatalogCommand
            {
                Id = role.Id,
                RoleName = role.RoleName,
                Notes = role.Notes
            };

            return View(command);
        }

        // POST: RolesCatalog/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateRolesCatalogCommand command)
        {
            if (id != command.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _rolesCatalogService.UpdateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating role: {ex.Message}");
                }
            }
            return View(command);
        }

        // GET: RolesCatalog/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var role = await _rolesCatalogService.GetByIdAsync(id.Value);
            if (role == null)
            {
                return NotFound();
            }

            return View(role);
        }

        // POST: RolesCatalog/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _rolesCatalogService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting role: {ex.Message}");
                var role = await _rolesCatalogService.GetByIdAsync(id);
                return View(role);
            }
        }
    }
}

