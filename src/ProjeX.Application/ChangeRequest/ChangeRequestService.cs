using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ProjeX.Application.ChangeRequest.Commands;
using ProjeX.Infrastructure.Data;
using ProjeX.Domain.Enums;

namespace ProjeX.Application.ChangeRequest
{
    public class ChangeRequestService : IChangeRequestService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ChangeRequestService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<ChangeRequestDto> RaiseAsync(RaiseChangeRequestCommand request)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == request.ProjectId && !p.IsDeleted);
            if (project == null)
            {
                throw new InvalidOperationException($"Project with ID {request.ProjectId} not found.");
            }

            var crCount = await _context.ChangeRequests.CountAsync();
            var requestNumber = $"CR-{DateTime.Now.Year}-{(crCount + 1):D4}";

            var changeRequest = new Domain.Entities.ChangeRequest
            {
                Id = Guid.NewGuid(),
                RequestNumber = requestNumber,
                ProjectId = request.ProjectId,
                Title = request.Title,
                Description = request.Description,
                Type = request.ChangeRequestType,
                Priority = request.Priority,
                Status = ChangeRequestStatus.Submitted,
                EstimatedCost = request.EstimatedCost,
                EstimatedHours = request.EstimatedHours,
                RequestedDate = DateTime.UtcNow,
                RequestedBy = request.RequestedBy,
                BusinessJustification = request.Justification,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.ChangeRequests.Add(changeRequest);
            await _context.SaveChangesAsync();

            var savedChangeRequest = await _context.ChangeRequests
                .Include(cr => cr.Project)
                .FirstOrDefaultAsync(cr => cr.Id == changeRequest.Id);

            return _mapper.Map<ChangeRequestDto>(savedChangeRequest);
        }

        public async Task<ChangeRequestDto> ProcessAsync(ProcessChangeRequestCommand request)
        {
            var changeRequest = await _context.ChangeRequests
                .Include(cr => cr.Project)
                .FirstOrDefaultAsync(cr => cr.Id == request.ChangeRequestId && !cr.IsDeleted);

            if (changeRequest == null)
            {
                throw new InvalidOperationException($"Change request with ID {request.ChangeRequestId} not found.");
            }

            if (changeRequest.Status != ChangeRequestStatus.Submitted && changeRequest.Status != ChangeRequestStatus.UnderReview)
            {
                throw new InvalidOperationException($"Only submitted or under review change requests can be processed. Current status: {changeRequest.Status}");
            }

            changeRequest.Status = request.NewStatus;
            changeRequest.ApprovalComments = request.ApprovalNotes;
            changeRequest.ApprovedBy = request.ApprovedBy;
            changeRequest.ApprovedDate = DateTime.UtcNow;
            changeRequest.ModifiedAt = DateTime.UtcNow;
            changeRequest.ModifiedBy = "system";

            if (request.NewStatus == ChangeRequestStatus.Approved && changeRequest.EstimatedCost > 0)
            {
                var project = changeRequest.Project;
                project.Budget += changeRequest.EstimatedCost;
                project.Notes += $"\n\nBudget increased by {changeRequest.EstimatedCost:C} due to approved change request {changeRequest.RequestNumber}";
                project.ModifiedAt = DateTime.UtcNow;
                project.ModifiedBy = "system";
            }

            await _context.SaveChangesAsync();

            return _mapper.Map<ChangeRequestDto>(changeRequest);
        }
    }
}

