using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Application.Invoice.Commands;
using LastMinute.Consultancy.Infrastructure.Data;
using ProjeX.Domain.Entities;
using ProjeX.Domain.Enums;

namespace ProjeX.Application.Invoice
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public InvoiceService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<InvoiceDto> PlanAsync(PlanInvoiceCommand request)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId && !p.IsDeleted);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {request.ProjectId} not found.");
            }

            var invoiceCount = await _context.Invoices.CountAsync();
            var invoiceNumber = $"INV-{DateTime.Now.Year}-{(invoiceCount + 1):D4}";

            var subTotal = request.LineItems.Sum(li => li.Quantity * li.UnitPrice);
            var taxAmount = subTotal * request.TaxRate;
            var totalAmount = subTotal + taxAmount;

            var invoice = new Domain.Entities.Invoice
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoiceNumber,
                ProjectId = request.ProjectId,
                ClientId = project.ClientId,
                InvoiceDate = request.InvoiceDate,
                DueDate = request.DueDate,
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                Status = InvoiceStatus.Planned,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.Invoices.Add(invoice);

            foreach (var lineItemRequest in request.LineItems)
            {
                var lineItem = new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    Description = lineItemRequest.Description,
                    Quantity = lineItemRequest.Quantity,
                    UnitRate = lineItemRequest.UnitPrice,
                    Amount = lineItemRequest.Quantity * lineItemRequest.UnitPrice,
                    Type = lineItemRequest.LineItemType,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "system"
                };
                _context.InvoiceLineItems.Add(lineItem);
            }

            await _context.SaveChangesAsync();

            var savedInvoice = await _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);

            return _mapper.Map<InvoiceDto>(savedInvoice);
        }

        public async Task<InvoiceDto> ConfirmAsync(ConfirmInvoiceCommand request)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && !i.IsDeleted);
            if (invoice == null)
            {
                throw new InvalidOperationException($"Invoice with ID {request.InvoiceId} not found.");
            }

            if (invoice.Status != InvoiceStatus.Planned && invoice.Status != InvoiceStatus.Draft)
            {
                throw new InvalidOperationException($"Only planned or draft invoices can be confirmed. Current status: {invoice.Status}");
            }

            invoice.Status = InvoiceStatus.Issued;
            invoice.Notes += $"\n\nConfirmed and issued on {DateTime.UtcNow:yyyy-MM-dd}: {request.ConfirmationNotes}";
            invoice.ModifiedAt = DateTime.UtcNow;
            invoice.ModifiedBy = "system";

            await _context.SaveChangesAsync();

            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<InvoiceDto> CancelAsync(CancelInvoiceCommand request)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && !i.IsDeleted);
            if (invoice == null)
            {
                throw new InvalidOperationException($"Invoice with ID {request.InvoiceId} not found.");
            }

            if (invoice.Status == InvoiceStatus.Paid || invoice.Status == InvoiceStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot cancel invoice with status: {invoice.Status}");
            }

            invoice.Status = InvoiceStatus.Cancelled;
            invoice.Notes += $"\n\nCancelled on {DateTime.UtcNow:yyyy-MM-dd}: {request.CancellationReason}";
            invoice.ModifiedAt = DateTime.UtcNow;
            invoice.ModifiedBy = "system";

            await _context.SaveChangesAsync();

            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<List<InvoiceDto>> GetAllAsync(Guid? projectId = null, Guid? clientId = null)
        {
            var query = _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .Where(i => !i.IsDeleted);

            if (projectId.HasValue)
            {
                query = query.Where(i => i.ProjectId == projectId.Value);
            }

            if (clientId.HasValue)
            {
                query = query.Where(i => i.ClientId == clientId.Value);
            }

            var invoices = await query.OrderByDescending(i => i.CreatedAt).ToListAsync();
            return _mapper.Map<List<InvoiceDto>>(invoices);
        }

        public async Task<InvoiceDto?> GetByIdAsync(Guid id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            return invoice != null ? _mapper.Map<InvoiceDto>(invoice) : null;
        }

        public async Task<InvoiceDto> UpdateAsync(Guid id, PlanInvoiceCommand command, string userId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (invoice == null)
            {
                throw new InvalidOperationException($"Invoice with ID {id} not found.");
            }

            if (invoice.Status != InvoiceStatus.Planned && invoice.Status != InvoiceStatus.Draft)
            {
                throw new InvalidOperationException($"Cannot update invoice with status: {invoice.Status}");
            }

            // Update invoice properties
            invoice.InvoiceDate = command.InvoiceDate;
            invoice.DueDate = command.DueDate;
            invoice.Notes = command.Notes;
            invoice.ModifiedAt = DateTime.UtcNow;
            invoice.ModifiedBy = userId;

            // Remove existing line items
            _context.InvoiceLineItems.RemoveRange(invoice.LineItems);

            // Calculate totals
            var subTotal = command.LineItems.Sum(li => li.Quantity * li.UnitPrice);
            var taxAmount = subTotal * command.TaxRate;
            var totalAmount = subTotal + taxAmount;

            invoice.SubTotal = subTotal;
            invoice.TaxAmount = taxAmount;
            invoice.TotalAmount = totalAmount;

            // Add new line items
            foreach (var lineItemRequest in command.LineItems)
            {
                var lineItem = new InvoiceLineItem
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    Description = lineItemRequest.Description,
                    Quantity = lineItemRequest.Quantity,
                    UnitRate = lineItemRequest.UnitPrice,
                    Amount = lineItemRequest.Quantity * lineItemRequest.UnitPrice,
                    Type = lineItemRequest.LineItemType,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                };
                _context.InvoiceLineItems.Add(lineItem);
            }

            await _context.SaveChangesAsync();

            var updatedInvoice = await _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == id);

            return _mapper.Map<InvoiceDto>(updatedInvoice);
        }

        public async Task<InvoiceDto> MarkAsSentAsync(Guid id, string userId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (invoice == null)
            {
                throw new InvalidOperationException($"Invoice with ID {id} not found.");
            }

            if (invoice.Status != InvoiceStatus.Issued)
            {
                throw new InvalidOperationException($"Only issued invoices can be marked as sent. Current status: {invoice.Status}");
            }

            invoice.Status = InvoiceStatus.Sent;
            invoice.Notes += $"\n\nMarked as sent on {DateTime.UtcNow:yyyy-MM-dd}";
            invoice.ModifiedAt = DateTime.UtcNow;
            invoice.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<InvoiceDto> MarkAsPaidAsync(Guid id, decimal amount, string paymentReference, string userId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (invoice == null)
            {
                throw new InvalidOperationException($"Invoice with ID {id} not found.");
            }

            if (invoice.Status == InvoiceStatus.Paid || invoice.Status == InvoiceStatus.Cancelled)
            {
                throw new InvalidOperationException($"Cannot mark invoice as paid. Current status: {invoice.Status}");
            }

            if (amount >= invoice.TotalAmount)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.PaidDate = DateTime.UtcNow;
            }
            else
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }

            invoice.PaymentReference = paymentReference;
            invoice.Notes += $"\n\nPayment recorded on {DateTime.UtcNow:yyyy-MM-dd}: ${amount:N2} - {paymentReference}";
            invoice.ModifiedAt = DateTime.UtcNow;
            invoice.ModifiedBy = userId;

            await _context.SaveChangesAsync();

            return _mapper.Map<InvoiceDto>(invoice);
        }

        public async Task<List<InvoiceDto>> GetOverdueInvoicesAsync()
        {
            var overdueInvoices = await _context.Invoices
                .Include(i => i.Project)
                .Include(i => i.Client)
                .Include(i => i.LineItems)
                .Where(i => !i.IsDeleted && 
                           i.DueDate < DateTime.Today && 
                           i.Status != InvoiceStatus.Paid && 
                           i.Status != InvoiceStatus.Cancelled)
                .OrderBy(i => i.DueDate)
                .ToListAsync();

            // Update status to overdue if not already set
            foreach (var invoice in overdueInvoices.Where(i => i.Status != InvoiceStatus.Overdue))
            {
                invoice.Status = InvoiceStatus.Overdue;
                invoice.ModifiedAt = DateTime.UtcNow;
                invoice.ModifiedBy = "system";
            }

            if (overdueInvoices.Any(i => i.Status == InvoiceStatus.Overdue))
            {
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<List<InvoiceDto>>(overdueInvoices);
        }

        public async Task<decimal> GetTotalReceivablesAsync()
        {
            return await _context.Invoices
                .Where(i => !i.IsDeleted && 
                           i.Status != InvoiceStatus.Paid && 
                           i.Status != InvoiceStatus.Cancelled)
                .SumAsync(i => i.TotalAmount);
        }

        public async Task<InvoiceDto> GenerateFromTimeEntriesAsync(Guid projectId, DateTime fromDate, DateTime toDate, string userId)
        {
            var project = await _context.Projects
                .Include(p => p.Client)
                .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {projectId} not found.");
            }

            // Get approved time entries for the project within date range
            var timeEntries = await _context.TimeEntries
                .Include(te => te.ActualAssignment)
                    .ThenInclude(aa => aa.Employee)
                .Include(te => te.ActualAssignment)
                    .ThenInclude(aa => aa.PlannedTeamSlot)
                        .ThenInclude(pts => pts.Role)
                .Where(te => te.ActualAssignment.ProjectId == projectId &&
                            te.Date >= fromDate &&
                            te.Date <= toDate &&
                            te.Status == TimeEntryStatus.Approved &&
                            te.IsBillable &&
                            te.Status != TimeEntryStatus.Invoiced)
                .ToListAsync();

            if (!timeEntries.Any())
            {
                throw new InvalidOperationException("No billable approved time entries found for the specified date range.");
            }

            // Group time entries by employee/role for line items
            var lineItems = timeEntries
                .GroupBy(te => new { 
                    EmployeeName = te.ActualAssignment.Employee.FullName,
                    RoleName = te.ActualAssignment.PlannedTeamSlot.Role.RoleName,
                    BillableRate = te.BillableRate ?? 0
                })
                .Select(g => new PlanInvoiceLineItem
                {
                    Description = $"{g.Key.RoleName} - {g.Key.EmployeeName} ({fromDate:MMM dd} - {toDate:MMM dd, yyyy})",
                    Quantity = g.Sum(te => te.Hours),
                    UnitPrice = g.Key.BillableRate,
                    LineItemType = LineItemType.Labor
                })
                .ToList();

            var command = new PlanInvoiceCommand
            {
                ProjectId = projectId,
                InvoiceDate = DateTime.Today,
                DueDate = DateTime.Today.AddDays(30),
                TaxRate = 0.10m, // 10% default
                Notes = $"Auto-generated invoice for time entries from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
                LineItems = lineItems
            };

            var invoice = await PlanAsync(command);

            // Mark time entries as invoiced
            foreach (var timeEntry in timeEntries)
            {
                timeEntry.Status = TimeEntryStatus.Invoiced;
                timeEntry.ModifiedAt = DateTime.UtcNow;
                timeEntry.ModifiedBy = userId;
            }

            await _context.SaveChangesAsync();

            return invoice;
        }
    }
}

