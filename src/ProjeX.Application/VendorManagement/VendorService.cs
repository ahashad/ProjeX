using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Enums;
using ProjeX.Infrastructure.Data;

namespace ProjeX.Application.VendorManagement
{
    public class VendorService : IVendorService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public VendorService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<VendorDto> CreateAsync(CreateVendorRequest request)
        {
            // Generate vendor code
            var vendorCode = await GenerateVendorCodeAsync();

            var vendor = _mapper.Map<Domain.Entities.Vendor>(request);
            vendor.Id = Guid.NewGuid();
            vendor.VendorCode = vendorCode;
            vendor.Status = VendorStatus.PendingApproval;

            _context.Vendors.Add(vendor);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(vendor.Id) ?? throw new InvalidOperationException("Failed to retrieve created vendor");
        }

        public async Task<VendorDto> UpdateAsync(UpdateVendorRequest request)
        {
            var vendor = await _context.Vendors.FindAsync(request.Id);
            if (vendor == null)
                throw new ArgumentException("Vendor not found");

            _mapper.Map(request, vendor);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(vendor.Id) ?? throw new InvalidOperationException("Failed to retrieve updated vendor");
        }

        public async Task<VendorDto?> GetByIdAsync(Guid id)
        {
            var vendor = await _context.Vendors
                .Include(v => v.Contracts)
                .Include(v => v.PurchaseOrders)
                .Include(v => v.VendorInvoices)
                .FirstOrDefaultAsync(v => v.Id == id);

            return vendor != null ? _mapper.Map<VendorDto>(vendor) : null;
        }

        public async Task<IEnumerable<VendorDto>> GetAllAsync(VendorStatus? status = null, VendorCategory? category = null)
        {
            var query = _context.Vendors.AsQueryable();

            if (status.HasValue)
                query = query.Where(v => v.Status == status.Value);

            if (category.HasValue)
                query = query.Where(v => v.Category == category.Value);

            var vendors = await query
                .Where(v => v.IsActive)
                .OrderBy(v => v.CompanyName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VendorDto>>(vendors);
        }

        public async Task<ThreeWayMatchResult> PerformThreeWayMatchAsync(Guid vendorInvoiceId)
        {
            var vendorInvoice = await _context.VendorInvoices
                .Include(vi => vi.Items)
                .Include(vi => vi.PurchaseOrder)
                    .ThenInclude(po => po.Items)
                .Include(vi => vi.GoodsReceipt)
                    .ThenInclude(gr => gr.Items)
                        .ThenInclude(gri => gri.PurchaseOrderItem)
                .FirstOrDefaultAsync(vi => vi.Id == vendorInvoiceId);

            if (vendorInvoice == null)
                throw new ArgumentException("Vendor invoice not found");

            var result = new ThreeWayMatchResult
            {
                VendorInvoiceId = vendorInvoiceId,
                IsMatched = true,
                MatchingDetails = new List<MatchingDetail>()
            };

            // Check if PO and GR exist
            if (vendorInvoice.PurchaseOrder == null)
            {
                result.IsMatched = false;
                result.Issues.Add("No purchase order linked to this invoice");
                return result;
            }

            if (vendorInvoice.GoodsReceipt == null)
            {
                result.IsMatched = false;
                result.Issues.Add("No goods receipt linked to this invoice");
                return result;
            }

            // Perform line-by-line matching
            foreach (var invoiceItem in vendorInvoice.Items)
            {
                var matchingDetail = new MatchingDetail
                {
                    LineNumber = invoiceItem.LineNumber,
                    ItemCode = invoiceItem.ItemCode,
                    Description = invoiceItem.Description
                };

                // Find corresponding PO item
                var poItem = vendorInvoice.PurchaseOrder.Items
                    .FirstOrDefault(poi => poi.ItemCode == invoiceItem.ItemCode);

                if (poItem == null)
                {
                    matchingDetail.Issues.Add("Item not found in purchase order");
                    result.IsMatched = false;
                }
                else
                {
                    matchingDetail.POQuantity = poItem.Quantity;
                    matchingDetail.POUnitPrice = poItem.UnitPrice;
                    matchingDetail.POLineTotal = poItem.LineTotal;

                    // Find corresponding GR item
                    var grItem = vendorInvoice.GoodsReceipt.Items
                        .FirstOrDefault(gri => gri.PurchaseOrderItemId == poItem.Id);

                    if (grItem == null)
                    {
                        matchingDetail.Issues.Add("Item not found in goods receipt");
                        result.IsMatched = false;
                    }
                    else
                    {
                        matchingDetail.GRQuantity = grItem.ReceivedQuantity;

                        // Check quantity matching
                        if (invoiceItem.Quantity > grItem.ReceivedQuantity)
                        {
                            matchingDetail.Issues.Add($"Invoice quantity ({invoiceItem.Quantity}) exceeds received quantity ({grItem.ReceivedQuantity})");
                            result.IsMatched = false;
                        }

                        // Check price matching (allow 5% tolerance)
                        var priceTolerance = poItem.UnitPrice * 0.05m;
                        if (Math.Abs(invoiceItem.UnitPrice - poItem.UnitPrice) > priceTolerance)
                        {
                            matchingDetail.Issues.Add($"Price variance exceeds tolerance. PO: {poItem.UnitPrice:C}, Invoice: {invoiceItem.UnitPrice:C}");
                            result.IsMatched = false;
                        }
                    }
                }

                matchingDetail.InvoiceQuantity = invoiceItem.Quantity;
                matchingDetail.InvoiceUnitPrice = invoiceItem.UnitPrice;
                matchingDetail.InvoiceLineTotal = invoiceItem.LineTotal;
                matchingDetail.IsMatched = !matchingDetail.Issues.Any();

                result.MatchingDetails.Add(matchingDetail);
            }

            // Update vendor invoice status
            vendorInvoice.IsThreeWayMatched = result.IsMatched;
            vendorInvoice.Status = result.IsMatched ? VendorInvoiceStatus.Matched : VendorInvoiceStatus.Disputed;
            vendorInvoice.MatchingNotes = result.IsMatched ? "Three-way match successful" : string.Join("; ", result.Issues);

            await _context.SaveChangesAsync();

            return result;
        }

        public async Task<VendorPerformanceDto> GetVendorPerformanceAsync(Guid vendorId, DateTime fromDate, DateTime toDate)
        {
            var vendor = await _context.Vendors.FindAsync(vendorId);
            if (vendor == null)
                throw new ArgumentException("Vendor not found");

            var purchaseOrders = await _context.PurchaseOrders
                .Include(po => po.Items)
                .Include(po => po.GoodsReceipts)
                .Where(po => po.VendorId == vendorId && 
                            po.OrderDate >= fromDate && 
                            po.OrderDate <= toDate)
                .ToListAsync();

            var vendorInvoices = await _context.VendorInvoices
                .Where(vi => vi.VendorId == vendorId && 
                            vi.InvoiceDate >= fromDate && 
                            vi.InvoiceDate <= toDate)
                .ToListAsync();

            var totalOrders = purchaseOrders.Count;
            var totalOrderValue = purchaseOrders.Sum(po => po.TotalAmount);
            var onTimeDeliveries = purchaseOrders.Count(po => 
                po.GoodsReceipts.Any(gr => gr.ReceiptDate <= po.RequiredDate));
            var totalInvoices = vendorInvoices.Count;
            var matchedInvoices = vendorInvoices.Count(vi => vi.IsThreeWayMatched);
            var averagePaymentDays = vendorInvoices
                .Where(vi => vi.PaymentDate.HasValue)
                .Average(vi => (vi.PaymentDate!.Value - vi.InvoiceDate).Days);

            return new VendorPerformanceDto
            {
                VendorId = vendorId,
                VendorName = vendor.CompanyName,
                PeriodStart = fromDate,
                PeriodEnd = toDate,
                TotalOrders = totalOrders,
                TotalOrderValue = totalOrderValue,
                OnTimeDeliveryRate = totalOrders > 0 ? (decimal)onTimeDeliveries / totalOrders * 100 : 0,
                TotalInvoices = totalInvoices,
                InvoiceMatchRate = totalInvoices > 0 ? (decimal)matchedInvoices / totalInvoices * 100 : 0,
                AveragePaymentDays = averagePaymentDays
            };
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var vendor = await _context.Vendors
                .Include(v => v.PurchaseOrders)
                .Include(v => v.VendorInvoices)
                .FirstOrDefaultAsync(v => v.Id == id);
            
            if (vendor == null)
                return false;

            // Check if vendor has active transactions
            var hasActiveTransactions = vendor.PurchaseOrders.Any(po => po.Status != PurchaseOrderStatus.Closed && po.Status != PurchaseOrderStatus.Cancelled) ||
                                       vendor.VendorInvoices.Any(vi => vi.Status != VendorInvoiceStatus.Paid);

            if (hasActiveTransactions)
                throw new InvalidOperationException("Cannot delete vendor with active transactions");

            vendor.IsActive = false;
            vendor.Status = VendorStatus.Inactive;
            await _context.SaveChangesAsync();

            return true;
        }

        private async Task<string> GenerateVendorCodeAsync()
        {
            var year = DateTime.UtcNow.Year.ToString().Substring(2);
            var lastVendor = await _context.Vendors
                .Where(v => v.VendorCode.StartsWith($"VEN{year}"))
                .OrderByDescending(v => v.VendorCode)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastVendor != null)
            {
                var numberPart = lastVendor.VendorCode.Substring(5);
                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"VEN{year}{nextNumber:D4}";
        }
    }
}

