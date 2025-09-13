using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Enums;
using ProjeX.Infrastructure.Data;

namespace ProjeX.Application.InvoicePlanning
{
    public class InvoicePlanService : IInvoicePlanService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public InvoicePlanService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<InvoicePlanDto> CreateAsync(CreateInvoicePlanRequest request)
        {
            // Validate project exists
            var project = await _context.Projects.FindAsync(request.ProjectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            // Validate contract value
            if (request.TotalContractValue > project.ContractValue)
                throw new InvalidOperationException("Invoice plan total cannot exceed project contract value");

            var invoicePlan = _mapper.Map<Domain.Entities.InvoicePlan>(request);
            invoicePlan.Id = Guid.NewGuid();

            // Calculate invoice details based on frequency
            CalculateInvoiceSchedule(invoicePlan);

            _context.InvoicePlans.Add(invoicePlan);
            await _context.SaveChangesAsync();

            // Generate invoice schedules
            await GenerateInvoiceSchedulesAsync(invoicePlan);

            return await GetByIdAsync(invoicePlan.Id) ?? throw new InvalidOperationException("Failed to retrieve created invoice plan");
        }

        public async Task<InvoicePlanDto> UpdateAsync(UpdateInvoicePlanRequest request)
        {
            var invoicePlan = await _context.InvoicePlans
                .Include(ip => ip.InvoiceSchedules)
                .FirstOrDefaultAsync(ip => ip.Id == request.Id);
            
            if (invoicePlan == null)
                throw new ArgumentException("Invoice plan not found");

            // Check if any invoices have been generated
            var hasGeneratedInvoices = invoicePlan.InvoiceSchedules.Any(s => s.GeneratedInvoiceId.HasValue);
            if (hasGeneratedInvoices)
                throw new InvalidOperationException("Cannot modify invoice plan with generated invoices");

            _mapper.Map(request, invoicePlan);
            
            // Recalculate schedule if frequency or dates changed
            CalculateInvoiceSchedule(invoicePlan);
            
            // Remove existing schedules and regenerate
            _context.InvoiceSchedules.RemoveRange(invoicePlan.InvoiceSchedules);
            await _context.SaveChangesAsync();
            
            await GenerateInvoiceSchedulesAsync(invoicePlan);

            return await GetByIdAsync(invoicePlan.Id) ?? throw new InvalidOperationException("Failed to retrieve updated invoice plan");
        }

        public async Task<InvoicePlanDto?> GetByIdAsync(Guid id)
        {
            var invoicePlan = await _context.InvoicePlans
                .Include(ip => ip.Project)
                .Include(ip => ip.InvoiceSchedules)
                    .ThenInclude(s => s.GeneratedInvoice)
                .Include(ip => ip.BillingRules)
                .FirstOrDefaultAsync(ip => ip.Id == id);

            return invoicePlan != null ? _mapper.Map<InvoicePlanDto>(invoicePlan) : null;
        }

        public async Task<IEnumerable<InvoicePlanDto>> GetByProjectIdAsync(Guid projectId)
        {
            var invoicePlans = await _context.InvoicePlans
                .Include(ip => ip.Project)
                .Include(ip => ip.InvoiceSchedules)
                .Include(ip => ip.BillingRules)
                .Where(ip => ip.ProjectId == projectId && ip.IsActive)
                .ToListAsync();

            return _mapper.Map<IEnumerable<InvoicePlanDto>>(invoicePlans);
        }

        public async Task<IEnumerable<InvoiceScheduleDto>> GetUpcomingInvoicesAsync(DateTime fromDate, DateTime toDate)
        {
            var schedules = await _context.InvoiceSchedules
                .Include(s => s.InvoicePlan)
                    .ThenInclude(ip => ip.Project)
                .Where(s => s.ScheduledDate >= fromDate && 
                           s.ScheduledDate <= toDate &&
                           s.Status == InvoiceScheduleStatus.Scheduled)
                .OrderBy(s => s.ScheduledDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<InvoiceScheduleDto>>(schedules);
        }

        public async Task<InvoiceGenerationResult> GenerateInvoiceAsync(Guid scheduleId)
        {
            var schedule = await _context.InvoiceSchedules
                .Include(s => s.InvoicePlan)
                    .ThenInclude(ip => ip.Project)
                        .ThenInclude(p => p.Client)
                .Include(s => s.InvoicePlan)
                    .ThenInclude(ip => ip.BillingRules)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
                throw new ArgumentException("Invoice schedule not found");

            if (schedule.Status != InvoiceScheduleStatus.Scheduled && schedule.Status != InvoiceScheduleStatus.Ready)
                throw new InvalidOperationException("Invoice schedule is not ready for generation");

            // Calculate invoice amount based on billing rules
            var calculatedAmount = await CalculateInvoiceAmountAsync(schedule);

            // Create invoice
            var invoice = new Domain.Entities.Invoice
            {
                Id = Guid.NewGuid(),
                ProjectId = schedule.InvoicePlan.ProjectId,
                ClientId = schedule.InvoicePlan.Project.ClientId,
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                InvoiceDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(GetPaymentTermsDays(schedule.InvoicePlan.PaymentTerms)),
                SubTotal = calculatedAmount,
                TaxAmount = calculatedAmount * (schedule.InvoicePlan.TaxRate / 100),
                TotalAmount = calculatedAmount + (calculatedAmount * (schedule.InvoicePlan.TaxRate / 100)),
                Currency = schedule.InvoicePlan.Currency,
                Status = InvoiceStatus.Draft,
                Description = schedule.Description,
                Notes = $"Generated from invoice plan: {schedule.InvoicePlan.PlanName}"
            };

            _context.Invoices.Add(invoice);

            // Update schedule
            schedule.Status = InvoiceScheduleStatus.Generated;
            schedule.GeneratedInvoiceId = invoice.Id;
            schedule.ActualInvoiceDate = DateTime.UtcNow;
            schedule.ActualAmount = calculatedAmount;

            await _context.SaveChangesAsync();

            return new InvoiceGenerationResult
            {
                IsSuccess = true,
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                Amount = invoice.TotalAmount
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var invoicePlan = await _context.InvoicePlans
                .Include(ip => ip.InvoiceSchedules)
                .FirstOrDefaultAsync(ip => ip.Id == id);
            
            if (invoicePlan == null)
                return false;

            // Check if any invoices have been generated
            var hasGeneratedInvoices = invoicePlan.InvoiceSchedules.Any(s => s.GeneratedInvoiceId.HasValue);
            if (hasGeneratedInvoices)
                throw new InvalidOperationException("Cannot delete invoice plan with generated invoices");

            invoicePlan.IsActive = false;
            await _context.SaveChangesAsync();

            return true;
        }

        private void CalculateInvoiceSchedule(Domain.Entities.InvoicePlan invoicePlan)
        {
            var totalDays = (invoicePlan.LastInvoiceDate - invoicePlan.FirstInvoiceDate).Days;
            
            invoicePlan.NumberOfInvoices = invoicePlan.Frequency switch
            {
                InvoiceFrequency.Weekly => (totalDays / 7) + 1,
                InvoiceFrequency.BiWeekly => (totalDays / 14) + 1,
                InvoiceFrequency.Monthly => (totalDays / 30) + 1,
                InvoiceFrequency.Quarterly => (totalDays / 90) + 1,
                InvoiceFrequency.SemiAnnually => (totalDays / 180) + 1,
                InvoiceFrequency.Annually => (totalDays / 365) + 1,
                _ => 1
            };

            if (invoicePlan.NumberOfInvoices > 0)
            {
                invoicePlan.InvoiceAmount = invoicePlan.TotalContractValue / invoicePlan.NumberOfInvoices;
            }
        }

        private async Task GenerateInvoiceSchedulesAsync(Domain.Entities.InvoicePlan invoicePlan)
        {
            var schedules = new List<Domain.Entities.InvoiceSchedule>();
            var currentDate = invoicePlan.FirstInvoiceDate;
            var sequenceNumber = 1;

            while (currentDate <= invoicePlan.LastInvoiceDate && sequenceNumber <= invoicePlan.NumberOfInvoices)
            {
                schedules.Add(new Domain.Entities.InvoiceSchedule
                {
                    Id = Guid.NewGuid(),
                    InvoicePlanId = invoicePlan.Id,
                    SequenceNumber = sequenceNumber,
                    ScheduledDate = currentDate,
                    ScheduledAmount = invoicePlan.InvoiceAmount,
                    Description = $"Invoice {sequenceNumber} of {invoicePlan.NumberOfInvoices}",
                    Status = InvoiceScheduleStatus.Scheduled
                });

                currentDate = GetNextInvoiceDate(currentDate, invoicePlan.Frequency);
                sequenceNumber++;
            }

            _context.InvoiceSchedules.AddRange(schedules);
            await _context.SaveChangesAsync();
        }

        private DateTime GetNextInvoiceDate(DateTime currentDate, InvoiceFrequency frequency)
        {
            return frequency switch
            {
                InvoiceFrequency.Weekly => currentDate.AddDays(7),
                InvoiceFrequency.BiWeekly => currentDate.AddDays(14),
                InvoiceFrequency.Monthly => currentDate.AddMonths(1),
                InvoiceFrequency.Quarterly => currentDate.AddMonths(3),
                InvoiceFrequency.SemiAnnually => currentDate.AddMonths(6),
                InvoiceFrequency.Annually => currentDate.AddYears(1),
                _ => currentDate.AddMonths(1)
            };
        }

        private async Task<decimal> CalculateInvoiceAmountAsync(Domain.Entities.InvoiceSchedule schedule)
        {
            decimal totalAmount = 0;

            foreach (var rule in schedule.InvoicePlan.BillingRules.Where(r => r.IsActive))
            {
                totalAmount += rule.RuleType switch
                {
                    BillingRuleType.FixedAmount => rule.FixedAmount ?? 0,
                    BillingRuleType.PercentageOfContract => (rule.PercentageOfContract ?? 0) * schedule.InvoicePlan.TotalContractValue / 100,
                    BillingRuleType.TimeAndMaterial => await CalculateTimeAndMaterialAsync(schedule),
                    _ => schedule.ScheduledAmount
                };
            }

            return totalAmount > 0 ? totalAmount : schedule.ScheduledAmount;
        }

        private async Task<decimal> CalculateTimeAndMaterialAsync(Domain.Entities.InvoiceSchedule schedule)
        {
            // Calculate based on time entries and expenses for the period
            var timeEntries = await _context.TimeEntries
                .Where(te => te.ActualAssignment.ProjectId == schedule.InvoicePlan.ProjectId &&
                            te.Date >= schedule.ScheduledDate.AddDays(-30) &&
                            te.Date <= schedule.ScheduledDate &&
      te.BillableRate.HasValue)
  .SumAsync(te => te.Hours * te.BillableRate.Value); // Use BillableRate instead of HourlyRate

            var expenses = await _context.Overheads
                .Where(o => o.ProjectId == schedule.InvoicePlan.ProjectId &&
                           o.CreatedAt >= schedule.ScheduledDate.AddDays(-30) &&
                           o.CreatedAt <= schedule.ScheduledDate)
                .SumAsync(o => o.Amount);

            return timeEntries + expenses;
        }

        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var lastInvoice = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith($"INV-{year}-"))
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastInvoice != null)
            {
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"INV-{year}-{nextNumber:D4}";
        }

        private int GetPaymentTermsDays(string paymentTerms)
        {
            return paymentTerms.ToLower() switch
            {
                "net 15" => 15,
                "net 30" => 30,
                "net 45" => 45,
                "net 60" => 60,
                "net 90" => 90,
                _ => 30
            };
        }
    }
}

