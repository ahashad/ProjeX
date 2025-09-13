using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjeX.Application.Deliverable.Commands;

namespace ProjeX.Application.Deliverable
{
    public interface IDeliverableService
    {
        Task<List<DeliverableDto>> GetAllAsync();
        Task<DeliverableDto?> GetByIdAsync(Guid id);
        Task<DeliverableDto> CreateAsync(CreateDeliverableCommand command, string userId);
        Task UpdateAsync(UpdateDeliverableCommand command, string userId);
        Task DeleteAsync(Guid id);
    }
}
