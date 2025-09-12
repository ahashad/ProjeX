using LastMinute.Consultancy.Application.PlannedTeamSlot.Commands;

namespace LastMinute.Consultancy.Application.PlannedTeamSlot
{
    public interface IPlannedTeamSlotService
    {
        Task<PlannedTeamSlotDto> CreateSlotAsync(CreatePlannedTeamSlotCommand command, string userId);
        Task<PlannedTeamSlotDto> UpdateSlotAsync(UpdatePlannedTeamSlotCommand command, string userId);
        Task<PlannedTeamSlotDto?> GetSlotByIdAsync(Guid id);
        Task<List<PlannedTeamSlotDto>> GetAvailableSlotsAsync(Guid projectId);
        Task<List<PlannedTeamSlotDto>> GetSlotsByProjectAsync(Guid projectId);
        Task DeleteSlotAsync(Guid id, string userId);
        Task RecalculateBudgetCostsAsync(Guid projectId);
        Task<Dictionary<Guid, decimal>> GetRemainingAllocationSegmentsAsync(Guid projectId);
    }
}