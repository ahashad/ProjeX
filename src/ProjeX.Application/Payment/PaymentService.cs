using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Application.Payment.Commands;
using LastMinute.Consultancy.Infrastructure.Data;
using ProjeX.Domain.Enums;

namespace ProjeX.Application.Payment
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public PaymentService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<PaymentDto> RecordAsync(RecordPaymentCommand request)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && !i.IsDeleted);
            if (invoice == null)
            {
                throw new InvalidOperationException($"Invoice with ID {request.InvoiceId} not found.");
            }

            if (invoice.Status != InvoiceStatus.Issued)
            {
                throw new InvalidOperationException($"Can only record payments for issued invoices. Current status: {invoice.Status}");
            }

            var existingPayments = await _context.Payments
                .Where(p => p.InvoiceId == request.InvoiceId && p.Status == PaymentStatus.Completed && !p.IsDeleted)
                .SumAsync(p => p.Amount);

            var remainingAmount = invoice.TotalAmount - existingPayments;
            if (request.Amount > remainingAmount)
            {
                throw new InvalidOperationException($"Payment amount ({request.Amount:C}) exceeds remaining balance ({remainingAmount:C}).");
            }

            var payment = new Domain.Entities.Payment
            {
                Id = Guid.NewGuid(),
                InvoiceId = request.InvoiceId,
                Amount = request.Amount,
                PaymentDate = request.PaymentDate,
                Method = request.PaymentMethod,
                Status = PaymentStatus.Completed,
                Reference = request.ReferenceNumber,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.Payments.Add(payment);

            var totalPayments = existingPayments + request.Amount;
            if (totalPayments >= invoice.TotalAmount)
            {
                invoice.Status = InvoiceStatus.Paid;
                invoice.ModifiedAt = DateTime.UtcNow;
                invoice.ModifiedBy = "system";
            }

            await _context.SaveChangesAsync();

            var savedPayment = await _context.Payments
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.Id == payment.Id);

            return _mapper.Map<PaymentDto>(savedPayment);
        }
    }
}

