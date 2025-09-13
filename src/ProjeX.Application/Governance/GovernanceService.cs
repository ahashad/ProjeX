using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Enums;
using ProjeX.Infrastructure.Data;

namespace ProjeX.Application.Governance
{
    public class GovernanceService : IGovernanceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GovernanceService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ApprovalWorkflowDto> CreateApprovalWorkflowAsync(CreateApprovalWorkflowRequest request)
        {
            var workflow = _mapper.Map<Domain.Entities.ApprovalWorkflow>(request);
            workflow.Id = Guid.NewGuid();
            workflow.IsActive = true;

            _context.ApprovalWorkflows.Add(workflow);
            await _context.SaveChangesAsync();

            return await GetApprovalWorkflowByIdAsync(workflow.Id) ?? throw new InvalidOperationException("Failed to retrieve created workflow");
        }

        public async Task<ApprovalWorkflowDto?> GetApprovalWorkflowByIdAsync(Guid id)
        {
            var workflow = await _context.ApprovalWorkflows
                .Include(w => w.Steps)
                    .ThenInclude(s => s.Approver)
                .FirstOrDefaultAsync(w => w.Id == id);

            return workflow != null ? _mapper.Map<ApprovalWorkflowDto>(workflow) : null;
        }

        public async Task<ApprovalRequestDto> SubmitForApprovalAsync(CreateApprovalRequest request)
        {
            // Get the appropriate workflow
            var workflow = await GetWorkflowForEntityAsync(request.EntityType, request.EntityValue);
            if (workflow == null)
                throw new InvalidOperationException($"No approval workflow found for {request.EntityType}");

            var approvalRequest = new Domain.Entities.ApprovalRequest
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                EntityType = request.EntityType,
                EntityId = request.EntityId,
                EntityValue = request.EntityValue,
                RequestedById = request.RequestedById,
                Status = ApprovalStatus.Pending,
                RequestDate = DateTime.UtcNow,
                Justification = request.Justification,
                CurrentStepNumber = 1
            };

            _context.ApprovalRequests.Add(approvalRequest);
            await _context.SaveChangesAsync();

            // Create first approval step
            await CreateApprovalStepAsync(approvalRequest.Id, 1);

            return await GetApprovalRequestByIdAsync(approvalRequest.Id) ?? throw new InvalidOperationException("Failed to retrieve created approval request");
        }

        public async Task<ApprovalRequestDto> ProcessApprovalAsync(Guid approvalRequestId, ProcessApprovalRequest request)
        {
            var approvalRequest = await _context.ApprovalRequests
                .Include(ar => ar.Workflow)
                    .ThenInclude(w => w.Steps)
                .Include(ar => ar.Steps)
                .FirstOrDefaultAsync(ar => ar.Id == approvalRequestId);

            if (approvalRequest == null)
                throw new ArgumentException("Approval request not found");

            var currentStep = approvalRequest.Steps
                .FirstOrDefault(s => s.StepNumber == approvalRequest.CurrentStepNumber && s.Status == ApprovalStatus.Pending);

            if (currentStep == null)
                throw new InvalidOperationException("No pending approval step found");

            // Process the current step
            currentStep.Status = request.IsApproved ? ApprovalStatus.Approved : ApprovalStatus.Rejected;
            currentStep.ApprovedById = request.ApprovedById;
            currentStep.ApprovedAt = DateTime.UtcNow;
            currentStep.Comments = request.Comments;

            if (request.IsApproved)
            {
                // Check if there are more steps
                var nextStepNumber = approvalRequest.CurrentStepNumber + 1;
                var hasNextStep = approvalRequest.Workflow.Steps.Any(s => s.StepNumber == nextStepNumber);

                if (hasNextStep)
                {
                    // Move to next step
                    approvalRequest.CurrentStepNumber = nextStepNumber;
                    await CreateApprovalStepAsync(approvalRequest.Id, nextStepNumber);
                }
                else
                {
                    // Final approval
                    approvalRequest.Status = ApprovalStatus.Approved;
                    approvalRequest.FinalApprovedAt = DateTime.UtcNow;
                    await ExecuteApprovedActionAsync(approvalRequest);
                }
            }
            else
            {
                // Rejection
                approvalRequest.Status = ApprovalStatus.Rejected;
                approvalRequest.FinalApprovedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return await GetApprovalRequestByIdAsync(approvalRequest.Id) ?? throw new InvalidOperationException("Failed to retrieve updated approval request");
        }

        public async Task<BudgetEncumbranceDto> CreateBudgetEncumbranceAsync(CreateBudgetEncumbranceRequest request)
        {
            // Check budget availability
            var budget = await _context.Budgets.FindAsync(request.BudgetId);
            if (budget == null)
                throw new ArgumentException("Budget not found");

            var existingEncumbrances = await _context.BudgetEncumbrances
                .Where(e => e.BudgetId == request.BudgetId && e.Status == EncumbranceStatus.Active)
                .SumAsync(e => e.Amount);

            var availableAmount = budget.AllocatedAmount - budget.SpentAmount - existingEncumbrances;
            if (request.Amount > availableAmount)
                throw new InvalidOperationException($"Insufficient budget. Available: {availableAmount:C}, Requested: {request.Amount:C}");

            var encumbrance = _mapper.Map<Domain.Entities.BudgetEncumbrance>(request);
            encumbrance.Id = Guid.NewGuid();
            encumbrance.Status = EncumbranceStatus.Active;
            encumbrance.EncumbranceDate = DateTime.UtcNow;

            _context.BudgetEncumbrances.Add(encumbrance);
            await _context.SaveChangesAsync();

            return await GetBudgetEncumbranceByIdAsync(encumbrance.Id) ?? throw new InvalidOperationException("Failed to retrieve created encumbrance");
        }

        public async Task<ChangeOrderDto> CreateChangeOrderAsync(CreateChangeOrderRequest request)
        {
            var changeOrderNumber = await GenerateChangeOrderNumberAsync();

            var changeOrder = _mapper.Map<Domain.Entities.ChangeOrder>(request);
            changeOrder.Id = Guid.NewGuid();
            changeOrder.ChangeOrderNumber = changeOrderNumber;
            changeOrder.Status = ChangeOrderStatus.Draft;

            _context.ChangeOrders.Add(changeOrder);
            await _context.SaveChangesAsync();

            // Submit for approval if required
            if (Math.Abs(changeOrder.CostImpact) > 1000) // Threshold for approval
            {
                await SubmitForApprovalAsync(new CreateApprovalRequest
                {
                    EntityType = "ChangeOrder",
                    EntityId = changeOrder.Id,
                    EntityValue = Math.Abs(changeOrder.CostImpact),
                    RequestedById = changeOrder.RequestedById,
                    Justification = changeOrder.Justification
                });

                changeOrder.Status = ChangeOrderStatus.PendingApproval;
                await _context.SaveChangesAsync();
            }

            return await GetChangeOrderByIdAsync(changeOrder.Id) ?? throw new InvalidOperationException("Failed to retrieve created change order");
        }

        public async Task<ComplianceReportDto> GenerateComplianceReportAsync(DateTime fromDate, DateTime toDate)
        {
            var approvalRequests = await _context.ApprovalRequests
                .Include(ar => ar.Workflow)
                .Where(ar => ar.RequestDate >= fromDate && ar.RequestDate <= toDate)
                .ToListAsync();

            var budgetEncumbrances = await _context.BudgetEncumbrances
                .Include(be => be.Budget)
                .Where(be => be.EncumbranceDate >= fromDate && be.EncumbranceDate <= toDate)
                .ToListAsync();

            var changeOrders = await _context.ChangeOrders
                .Include(co => co.Project)
                .Where(co => co.RequestDate >= fromDate && co.RequestDate <= toDate)
                .ToListAsync();

            var report = new ComplianceReportDto
            {
                ReportPeriodStart = fromDate,
                ReportPeriodEnd = toDate,
                TotalApprovalRequests = approvalRequests.Count,
                ApprovedRequests = approvalRequests.Count(ar => ar.Status == ApprovalStatus.Approved),
                RejectedRequests = approvalRequests.Count(ar => ar.Status == ApprovalStatus.Rejected),
                PendingRequests = approvalRequests.Count(ar => ar.Status == ApprovalStatus.Pending),
                TotalBudgetEncumbrances = budgetEncumbrances.Count,
                TotalEncumberedAmount = budgetEncumbrances.Sum(be => be.Amount),
                TotalChangeOrders = changeOrders.Count,
                TotalChangeOrderValue = changeOrders.Sum(co => Math.Abs(co.CostImpact)),
                ApprovalsByType = approvalRequests
                    .GroupBy(ar => ar.EntityType)
                    .Select(g => new ApprovalSummaryDto
                    {
                        EntityType = g.Key,
                        TotalRequests = g.Count(),
                        ApprovedCount = g.Count(ar => ar.Status == ApprovalStatus.Approved),
                        RejectedCount = g.Count(ar => ar.Status == ApprovalStatus.Rejected),
                        PendingCount = g.Count(ar => ar.Status == ApprovalStatus.Pending),
                        AverageProcessingDays = g.Where(ar => ar.FinalApprovedAt.HasValue)
                            .Average(ar => (ar.FinalApprovedAt!.Value - ar.RequestDate).TotalDays)
                    })
                    .ToList()
            };

            return report;
        }

        private async Task<Domain.Entities.ApprovalWorkflow?> GetWorkflowForEntityAsync(string entityType, decimal entityValue)
        {
            return await _context.ApprovalWorkflows
                .Include(w => w.Steps)
                    .ThenInclude(s => s.Approver)
                .FirstOrDefaultAsync(w => w.EntityType == entityType &&
                                        w.MinValue <= entityValue &&
                                        (w.MaxValue == null || w.MaxValue >= entityValue) &&
                                        w.IsActive);
        }

        private async Task CreateApprovalStepAsync(Guid approvalRequestId, int stepNumber)
        {
            var approvalRequest = await _context.ApprovalRequests
                .Include(ar => ar.Workflow)
                    .ThenInclude(w => w.Steps)
                .FirstOrDefaultAsync(ar => ar.Id == approvalRequestId);

            if (approvalRequest == null) return;

            var workflowStep = approvalRequest.Workflow.Steps
                .FirstOrDefault(s => s.StepNumber == stepNumber);

            if (workflowStep == null) return;

            var approvalStep = new Domain.Entities.ApprovalStep
            {
                Id = Guid.NewGuid(),
                ApprovalRequestId = approvalRequestId,
                StepNumber = stepNumber,
                ApproverId = workflowStep.ApproverId,
                Status = ApprovalStatus.Pending,
                DueDate = DateTime.UtcNow.AddDays(workflowStep.DueDays ?? 3)
            };

            _context.ApprovalSteps.Add(approvalStep);
            await _context.SaveChangesAsync();
        }

        private async Task ExecuteApprovedActionAsync(Domain.Entities.ApprovalRequest approvalRequest)
        {
            // Execute entity-specific approved actions
            switch (approvalRequest.EntityType)
            {
                case "Budget":
                    await ApproveBudgetAsync(approvalRequest.EntityId);
                    break;
                case "ChangeOrder":
                    await ApproveChangeOrderAsync(approvalRequest.EntityId);
                    break;
                case "PurchaseOrder":
                    await ApprovePurchaseOrderAsync(approvalRequest.EntityId);
                    break;
                // Add more entity types as needed
            }
        }

        private async Task ApproveBudgetAsync(Guid budgetId)
        {
            var budget = await _context.Budgets.FindAsync(budgetId);
            if (budget != null)
            {
                budget.Status = BudgetStatus.Approved;
                await _context.SaveChangesAsync();
            }
        }

        private async Task ApproveChangeOrderAsync(Guid changeOrderId)
        {
            var changeOrder = await _context.ChangeOrders
                .Include(co => co.Project)
                .FirstOrDefaultAsync(co => co.Id == changeOrderId);
            
            if (changeOrder != null)
            {
                changeOrder.Status = ChangeOrderStatus.Approved;
                changeOrder.ApprovedAt = DateTime.UtcNow;

                // Update project budget if applicable
                if (changeOrder.Project != null)
                {
                    changeOrder.Project.ContractValue += changeOrder.CostImpact;
                    if (changeOrder.ScheduleImpactDays != 0)
                    {
                        changeOrder.Project.EndDate = changeOrder.Project.EndDate.AddDays(changeOrder.ScheduleImpactDays);
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        private async Task ApprovePurchaseOrderAsync(Guid purchaseOrderId)
        {
            var purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder != null)
            {
                purchaseOrder.Status = PurchaseOrderStatus.Approved;
                purchaseOrder.ApprovedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        private async Task<string> GenerateChangeOrderNumberAsync()
        {
            var year = DateTime.UtcNow.Year.ToString().Substring(2);
            var lastChangeOrder = await _context.ChangeOrders
                .Where(co => co.ChangeOrderNumber.StartsWith($"CO{year}"))
                .OrderByDescending(co => co.ChangeOrderNumber)
                .FirstOrDefaultAsync();

            var nextNumber = 1;
            if (lastChangeOrder != null)
            {
                var numberPart = lastChangeOrder.ChangeOrderNumber.Substring(4);
                if (int.TryParse(numberPart, out var lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"CO{year}{nextNumber:D4}";
        }

        public async Task<ApprovalRequestDto?> GetApprovalRequestByIdAsync(Guid id)
        {
            var request = await _context.ApprovalRequests
                .Include(ar => ar.Workflow)
                .Include(ar => ar.Steps)
                    .ThenInclude(s => s.Approver)
                .Include(ar => ar.RequestedBy)
                .FirstOrDefaultAsync(ar => ar.Id == id);

            return request != null ? _mapper.Map<ApprovalRequestDto>(request) : null;
        }

        public async Task<BudgetEncumbranceDto?> GetBudgetEncumbranceByIdAsync(Guid id)
        {
            var encumbrance = await _context.BudgetEncumbrances
                .Include(be => be.Budget)
                .Include(be => be.Project)
                .FirstOrDefaultAsync(be => be.Id == id);

            return encumbrance != null ? _mapper.Map<BudgetEncumbranceDto>(encumbrance) : null;
        }

        public async Task<ChangeOrderDto?> GetChangeOrderByIdAsync(Guid id)
        {
            var changeOrder = await _context.ChangeOrders
                .Include(co => co.Project)
                .Include(co => co.RequestedBy)
                .FirstOrDefaultAsync(co => co.Id == id);

            return changeOrder != null ? _mapper.Map<ChangeOrderDto>(changeOrder) : null;
        }
    }
}

