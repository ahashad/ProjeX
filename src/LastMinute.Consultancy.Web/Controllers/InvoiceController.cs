using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using LastMinute.Consultancy.Application.Invoice;
using LastMinute.Consultancy.Application.Invoice.Commands;
using LastMinute.Consultancy.Application.Project;
using LastMinute.Consultancy.Application.Client;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Web.Controllers
{
    [Authorize(Policy = "ManagerOrAdmin")]
    public class InvoiceController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IProjectService _projectService;
        private readonly IClientService _clientService;

        public InvoiceController(IInvoiceService invoiceService, IProjectService projectService, IClientService clientService)
        {
            _invoiceService = invoiceService;
            _projectService = projectService;
            _clientService = clientService;
        }

        // GET: Invoice
        public async Task<IActionResult> Index(Guid? projectId, Guid? clientId, string status = "")
        {
            var invoices = await _invoiceService.GetAllAsync(projectId, clientId);

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<InvoiceStatus>(status, out var statusEnum))
            {
                invoices = invoices.Where(i => i.Status == statusEnum.ToString()).ToList();
            }

            // Populate filter dropdowns
            await PopulateFilterDropdowns(projectId, clientId);
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedProjectId = projectId;
            ViewBag.SelectedClientId = clientId;

            return View(invoices);
        }

        // GET: Invoice/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _invoiceService.GetByIdAsync(id.Value);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // GET: Invoice/Create
        public async Task<IActionResult> Create(Guid? projectId)
        {
            await PopulateDropdowns(projectId);

            var model = new PlanInvoiceCommand();
            if (projectId.HasValue)
            {
                model.ProjectId = projectId.Value;
            }

            return View(model);
        }

        // POST: Invoice/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PlanInvoiceCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _invoiceService.PlanAsync(command);
                    TempData["Success"] = "Invoice created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            await PopulateDropdowns(command.ProjectId);
            return View(command);
        }

        // GET: Invoice/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _invoiceService.GetByIdAsync(id.Value);
            if (invoice == null)
            {
                return NotFound();
            }

            // Check if invoice can be edited
            if (invoice.Status != InvoiceStatus.Planned.ToString() && invoice.Status != InvoiceStatus.Draft.ToString())
            {
                TempData["Error"] = "Only planned or draft invoices can be edited.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var command = new PlanInvoiceCommand
            {
                ProjectId = invoice.ProjectId,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                Notes = invoice.Notes,
                LineItems = invoice.LineItems.Select(li => new PlanInvoiceLineItem
                {
                    Description = li.Description,
                    Quantity = li.Quantity,
                    UnitPrice = li.UnitPrice,
                    LineItemType = Enum.Parse<LineItemType>(li.LineItemType)
                }).ToList()
            };

            // Calculate tax rate from existing data
            if (invoice.SubTotal > 0)
            {
                command.TaxRate = invoice.TaxAmount / invoice.SubTotal;
            }

            await PopulateDropdowns(command.ProjectId);
            ViewBag.InvoiceId = id;

            return View(command);
        }

        // POST: Invoice/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, PlanInvoiceCommand command)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _invoiceService.UpdateAsync(id, command, User.Identity?.Name ?? "current-user");
                    TempData["Success"] = "Invoice updated successfully.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            await PopulateDropdowns(command.ProjectId);
            ViewBag.InvoiceId = id;
            return View(command);
        }

        // POST: Invoice/Confirm/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(Guid id, string confirmationNotes)
        {
            try
            {
                var command = new ConfirmInvoiceCommand
                {
                    InvoiceId = id,
                    ConfirmationNotes = confirmationNotes ?? ""
                };

                await _invoiceService.ConfirmAsync(command);
                TempData["Success"] = "Invoice has been confirmed and issued.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Invoice/MarkAsSent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsSent(Guid id)
        {
            try
            {
                await _invoiceService.MarkAsSentAsync(id, User.Identity?.Name ?? "current-user");
                TempData["Success"] = "Invoice has been marked as sent.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Invoice/MarkAsPaid/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(Guid id, decimal amount, string paymentReference)
        {
            try
            {
                await _invoiceService.MarkAsPaidAsync(id, amount, paymentReference ?? "", User.Identity?.Name ?? "current-user");
                TempData["Success"] = "Payment has been recorded successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Invoice/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id, string cancellationReason)
        {
            try
            {
                var command = new CancelInvoiceCommand
                {
                    InvoiceId = id,
                    CancellationReason = cancellationReason ?? "No reason provided"
                };

                await _invoiceService.CancelAsync(command);
                TempData["Success"] = "Invoice has been cancelled.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Invoice/Overdue
        public async Task<IActionResult> Overdue()
        {
            var overdueInvoices = await _invoiceService.GetOverdueInvoicesAsync();
            return View(overdueInvoices);
        }

        // GET: Invoice/GenerateFromTimeEntries
        public async Task<IActionResult> GenerateFromTimeEntries(Guid? projectId)
        {
            await PopulateDropdowns(projectId);

            var model = new GenerateInvoiceFromTimeEntriesViewModel
            {
                ProjectId = projectId ?? Guid.Empty,
                FromDate = DateTime.Today.AddDays(-30),
                ToDate = DateTime.Today
            };

            return View(model);
        }

        // POST: Invoice/GenerateFromTimeEntries
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateFromTimeEntries(GenerateInvoiceFromTimeEntriesViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var invoice = await _invoiceService.GenerateFromTimeEntriesAsync(
                        model.ProjectId, 
                        model.FromDate, 
                        model.ToDate, 
                        User.Identity?.Name ?? "current-user"
                    );

                    TempData["Success"] = "Invoice generated successfully from time entries.";
                    return RedirectToAction(nameof(Details), new { id = invoice.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            await PopulateDropdowns(model.ProjectId);
            return View(model);
        }

        // AJAX: Get project details for auto-population
        [HttpGet]
        public async Task<IActionResult> GetProjectDetails(Guid projectId)
        {
            try
            {
                var projects = await _projectService.GetAllAsync();
                var project = projects.FirstOrDefault(p => p.Id == projectId);

                if (project == null)
                {
                    return NotFound();
                }

                return Json(new
                {
                    projectName = project.ProjectName,
                    clientId = project.ClientId,
                    clientName = project.ClientName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private async Task PopulateDropdowns(Guid? selectedProjectId = null)
        {
            // Get projects
            var projects = await _projectService.GetAllAsync();
            ViewBag.ProjectId = new SelectList(projects, "Id", "ProjectName", selectedProjectId);

            // Get line item types
            ViewBag.LineItemTypes = new SelectList(
                Enum.GetValues<LineItemType>()
                    .Select(e => new { Value = (int)e, Text = e.ToString() }),
                "Value",
                "Text"
            );
        }

        private async Task PopulateFilterDropdowns(Guid? selectedProjectId = null, Guid? selectedClientId = null)
        {
            // Get projects for filter
            var projects = await _projectService.GetAllAsync();
            ViewBag.FilterProjectId = new SelectList(
                projects.Select(p => new { Id = p.Id, Name = p.ProjectName }),
                "Id", "Name", selectedProjectId);

            // Get clients for filter
            var clients = await _clientService.GetAllAsync();
            ViewBag.FilterClientId = new SelectList(
                clients.Select(c => new { Id = c.Id, Name = c.ClientName }),
                "Id", "Name", selectedClientId);

            // Get invoice statuses for filter
            ViewBag.FilterStatuses = new SelectList(
                Enum.GetValues<InvoiceStatus>()
                    .Select(e => new { Value = e.ToString(), Text = e.ToString() }),
                "Value", "Text");
        }
    }

    // Helper view model for generating invoices from time entries
    public class GenerateInvoiceFromTimeEntriesViewModel
    {
        public Guid ProjectId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }
}