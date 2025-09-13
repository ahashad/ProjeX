using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Enums;
using ProjeX.Infrastructure.Data;

namespace ProjeX.Application.CRM
{
    public class CrmService : ICrmService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CrmService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Account Management
        public async Task<AccountDto> CreateAccountAsync(CreateAccountRequest request)
        {
            var accountNumber = await GenerateAccountNumberAsync();

            var account = _mapper.Map<Domain.Entities.Account>(request);
            account.Id = Guid.NewGuid();
            account.AccountNumber = accountNumber;
            account.Status = AccountStatus.Prospect;

            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();

            return await GetAccountByIdAsync(account.Id) ?? throw new InvalidOperationException("Failed to retrieve created account");
        }

        public async Task<AccountDto?> GetAccountByIdAsync(Guid id)
        {
            var account = await _context.Accounts
                .Include(a => a.AccountManager)
                .Include(a => a.Contacts)
                .Include(a => a.Opportunities)
                .Include(a => a.Projects)
                .FirstOrDefaultAsync(a => a.Id == id);

            return account != null ? _mapper.Map<AccountDto>(account) : null;
        }

        public async Task<IEnumerable<AccountDto>> GetAccountsAsync(AccountStatus? status = null, string? industry = null)
        {
            var query = _context.Accounts.AsQueryable();

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (!string.IsNullOrEmpty(industry))
                query = query.Where(a => a.Industry.Contains(industry));

            var accounts = await query
                .Include(a => a.AccountManager)
                .Where(a => a.IsActive)
                .OrderBy(a => a.CompanyName)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AccountDto>>(accounts);
        }

        // Opportunity Management
        public async Task<OpportunityDto> CreateOpportunityAsync(CreateOpportunityRequest request)
        {
            var opportunityNumber = await GenerateOpportunityNumberAsync();

            var opportunity = _mapper.Map<Domain.Entities.Opportunity>(request);
            opportunity.Id = Guid.NewGuid();
            opportunity.OpportunityNumber = opportunityNumber;
            opportunity.Stage = OpportunityStage.Qualification;

            _context.Opportunities.Add(opportunity);
            await _context.SaveChangesAsync();

            return await GetOpportunityByIdAsync(opportunity.Id) ?? throw new InvalidOperationException("Failed to retrieve created opportunity");
        }

        public async Task<OpportunityDto?> GetOpportunityByIdAsync(Guid id)
        {
            var opportunity = await _context.Opportunities
                .Include(o => o.Account)
                .Include(o => o.PrimaryContact)
                .Include(o => o.Owner)
                .Include(o => o.Activities)
                .FirstOrDefaultAsync(o => o.Id == id);

            return opportunity != null ? _mapper.Map<OpportunityDto>(opportunity) : null;
        }

        public async Task<OpportunityDto> UpdateOpportunityStageAsync(Guid id, OpportunityStage newStage, string notes = "")
        {
            var opportunity = await _context.Opportunities.FindAsync(id);
            if (opportunity == null)
                throw new ArgumentException("Opportunity not found");

            var oldStage = opportunity.Stage;
            opportunity.Stage = newStage;
            opportunity.LastActivityDate = DateTime.UtcNow;

            // Handle stage-specific logic
            if (newStage == OpportunityStage.ClosedWon || newStage == OpportunityStage.ClosedLost)
            {
                opportunity.ActualCloseDate = DateTime.UtcNow;
                opportunity.IsActive = false;

                if (newStage == OpportunityStage.ClosedWon)
                {
                    // Update account status to customer
                    var account = await _context.Accounts.FindAsync(opportunity.AccountId);
                    if (account != null && account.Status == AccountStatus.Prospect)
                    {
                        account.Status = AccountStatus.Customer;
                    }
                }
            }

            // Log activity
            var activity = new Domain.Entities.OpportunityActivity
            {
                Id = Guid.NewGuid(),
                OpportunityId = id,
                ActivityType = ActivityType.Note,
                Subject = $"Stage changed from {oldStage} to {newStage}",
                Description = notes,
                ActivityDate = DateTime.UtcNow
            };

            _context.OpportunityActivities.Add(activity);
            await _context.SaveChangesAsync();

            return await GetOpportunityByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated opportunity");
        }

        public async Task<SalesPipelineDto> GetSalesPipelineAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Opportunities.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(o => o.CreatedAt <= toDate.Value);

            var opportunities = await query
                .Include(o => o.Account)
                .Include(o => o.Owner)
                .Where(o => o.IsActive)
                .ToListAsync();

            var pipeline = new SalesPipelineDto
            {
                TotalOpportunities = opportunities.Count,
                TotalValue = opportunities.Sum(o => o.EstimatedValue),
                WeightedValue = opportunities.Sum(o => o.EstimatedValue * o.ProbabilityPercent / 100),
                StageBreakdown = opportunities
                    .GroupBy(o => o.Stage)
                    .Select(g => new PipelineStageDto
                    {
                        Stage = g.Key,
                        StageName = g.Key.ToString(),
                        Count = g.Count(),
                        Value = g.Sum(o => o.EstimatedValue),
                        WeightedValue = g.Sum(o => o.EstimatedValue * o.ProbabilityPercent / 100)
                    })
                    .OrderBy(s => (int)s.Stage)
                    .ToList()
            };

            return pipeline;
        }

        // Tender Management
        public async Task<TenderDto> CreateTenderAsync(CreateTenderRequest request)
        {
            var tenderNumber = await GenerateTenderNumberAsync();

            var tender = _mapper.Map<Domain.Entities.Tender>(request);
            tender.Id = Guid.NewGuid();
            tender.TenderNumber = tenderNumber;
            tender.Status = TenderStatus.Published;

            _context.Tenders.Add(tender);
            await _context.SaveChangesAsync();

            return await GetTenderByIdAsync(tender.Id) ?? throw new InvalidOperationException("Failed to retrieve created tender");
        }

        public async Task<TenderDto?> GetTenderByIdAsync(Guid id)
        {
            var tender = await _context.Tenders
                .Include(t => t.Account)
                .Include(t => t.AssignedTo)
                .Include(t => t.Documents)
                .Include(t => t.Activities)
                .FirstOrDefaultAsync(t => t.Id == id);

            return tender != null ? _mapper.Map<TenderDto>(tender) : null;
        }

        public async Task<IEnumerable<TenderDto>> GetTendersAsync(TenderStatus? status = null, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Tenders.AsQueryable();

            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (fromDate.HasValue)
                query = query.Where(t => t.PublishedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.SubmissionDeadline <= toDate.Value);

            var tenders = await query
                .Include(t => t.Account)
                .Include(t => t.AssignedTo)
                .OrderByDescending(t => t.PublishedDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<TenderDto>>(tenders);
        }

        public async Task<TenderDto> SubmitTenderAsync(Guid id, decimal bidAmount, string notes = "")
        {
            var tender = await _context.Tenders.FindAsync(id);
            if (tender == null)
                throw new ArgumentException("Tender not found");

            if (tender.SubmissionDeadline < DateTime.UtcNow)
                throw new InvalidOperationException("Submission deadline has passed");

            tender.Status = TenderStatus.Submitted;
            tender.SubmittedBidAmount = bidAmount;
            tender.SubmittedDate = DateTime.UtcNow;

            // Log activity
            var activity = new Domain.Entities.TenderActivity
            {
                Id = Guid.NewGuid(),
                TenderId = id,
                ActivityType = ActivityType.Note,
                Subject = "Tender Submitted",
                Description = $"Bid amount: {bidAmount:C}. {notes}",
                ActivityDate = DateTime.UtcNow
            };

            _context.TenderActivities.Add(activity);
            await _context.SaveChangesAsync();

            return await GetTenderByIdAsync(id) ?? throw new InvalidOperationException("Failed to retrieve updated tender");
        }

        public async Task<CrmDashboardDto> GetDashboardAsync()
        {
            var totalAccounts = await _context.Accounts.CountAsync(a => a.IsActive);
            var totalOpportunities = await _context.Opportunities.CountAsync(o => o.IsActive);
            var totalTenders = await _context.Tenders.CountAsync();

            var pipelineValue = await _context.Opportunities
                .Where(o => o.IsActive)
                .SumAsync(o => o.EstimatedValue);

            var wonOpportunities = await _context.Opportunities
                .CountAsync(o => o.Stage == OpportunityStage.ClosedWon);

            var activeTenders = await _context.Tenders
                .CountAsync(t => t.Status == TenderStatus.InPreparation || t.Status == TenderStatus.Submitted);

            var recentActivities = await _context.OpportunityActivities
                .Include(a => a.Opportunity)
                    .ThenInclude(o => o.Account)
                .OrderByDescending(a => a.ActivityDate)
                .Take(10)
                .ToListAsync();

            return new CrmDashboardDto
            {
                TotalAccounts = totalAccounts,
                TotalOpportunities = totalOpportunities,
                TotalTenders = totalTenders,
                PipelineValue = pipelineValue,
                WonOpportunities = wonOpportunities,
                ActiveTenders = activeTenders,
                RecentActivities = _mapper.Map<List<ActivityDto>>(recentActivities)
            };
        }

        private async Task<string> GenerateAccountNumberAsync()
        {
            var year = DateTime.UtcNow.Year.ToString().Substring(2);
            var lastAccount = await _context.Accounts
                .Where(a => a.AccountNumber.StartsWith($"ACC{year}"))
                .OrderByDescending(a => a.AccountNumber)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastAccount != null)
            {
                var numberPart = lastAccount.AccountNumber.Substring(5);
                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"ACC{year}{nextNumber:D4}";
        }

        private async Task<string> GenerateOpportunityNumberAsync()
        {
            var year = DateTime.UtcNow.Year.ToString().Substring(2);
            var lastOpportunity = await _context.Opportunities
                .Where(o => o.OpportunityNumber.StartsWith($"OPP{year}"))
                .OrderByDescending(o => o.OpportunityNumber)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastOpportunity != null)
            {
                var numberPart = lastOpportunity.OpportunityNumber.Substring(5);
                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"OPP{year}{nextNumber:D4}";
        }

        private async Task<string> GenerateTenderNumberAsync()
        {
            var year = DateTime.UtcNow.Year.ToString().Substring(2);
            var lastTender = await _context.Tenders
                .Where(t => t.TenderNumber.StartsWith($"TEN{year}"))
                .OrderByDescending(t => t.TenderNumber)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastTender != null)
            {
                var numberPart = lastTender.TenderNumber.Substring(5);
                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"TEN{year}{nextNumber:D4}";
        }
    }
}

