using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using LastMinute.Consultancy.Application.Employee;
using LastMinute.Consultancy.Application.Employee.Commands;
using LastMinute.Consultancy.Application.RolesCatalog;
using LastMinute.Consultancy.Application.ActualAssignment;

namespace LastMinute.Consultancy.Web.Controllers
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IRolesCatalogService _rolesCatalogService;
        private readonly IAssignmentService _assignmentService;

        public EmployeeController(IEmployeeService employeeService, IRolesCatalogService rolesCatalogService, IAssignmentService assignmentService)
        {
            _employeeService = employeeService;
            _rolesCatalogService = rolesCatalogService;
            _assignmentService = assignmentService;
        }

        // GET: Employee
        public async Task<IActionResult> Index()
        {
            var employees = await _employeeService.GetAllAsync();
            return View(employees);
        }

        // GET: Employee/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _employeeService.GetByIdAsync(id.Value);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpGet]
        public async Task<IActionResult> Utilization(Guid id, DateTime? from, DateTime? to)
        {
            var start = from ?? DateTime.UtcNow.Date.AddDays(-30);
            var end = to ?? DateTime.UtcNow.Date;
            var data = await _assignmentService.GetEmployeeUtilizationAsync(id, start, end);
            var response = data.Select(d => new { date = d.Date.ToString("yyyy-MM-dd"), allocationPercent = d.AllocationPercent });
            return Json(response);
        }

        // GET: Employee/Create
        public async Task<IActionResult> Create()
        {
            await PopulateRolesDropdown();
            return View();
        }

        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _employeeService.CreateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating employee: {ex.Message}");
                }
            }
            await PopulateRolesDropdown(command.RoleId);
            return View(command);
        }

        // GET: Employee/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _employeeService.GetByIdAsync(id.Value);
            if (employee == null)
            {
                return NotFound();
            }

            var command = new UpdateEmployeeCommand
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Phone = employee.Phone,
                RoleId = employee.RoleId,
                Salary = employee.Salary,
                MonthlyIncentive = employee.MonthlyIncentive,
                CommissionPercent = employee.CommissionPercent,
                HireDate = employee.HireDate,
                IsActive = employee.IsActive
            };

            await PopulateRolesDropdown(command.RoleId);
            return View(command);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UpdateEmployeeCommand command)
        {
            if (id != command.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _employeeService.UpdateAsync(command);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error updating employee: {ex.Message}");
                }
            }
            await PopulateRolesDropdown(command.RoleId);
            return View(command);
        }

        // GET: Employee/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _employeeService.GetByIdAsync(id.Value);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                await _employeeService.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error deleting employee: {ex.Message}");
                var employee = await _employeeService.GetByIdAsync(id);
                return View(employee);
            }
        }

        private async Task PopulateRolesDropdown(Guid? selectedRoleId = null)
        {
            var roles = await _rolesCatalogService.GetAllAsync();
            ViewBag.RoleId = new SelectList(roles, "Id", "RoleName", selectedRoleId);
        }
    }
}

