using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LastMinute.Consultancy.Application.Deliverable.Commands;

namespace LastMinute.Consultancy.Application.Deliverable
{
    public interface IDeliverableService
    {
        Task<List<DeliverableDto>> GetAllAsync();
        Task<DeliverableDto?> GetByIdAsync(Guid id);
        Task<DeliverableDto> CreateAsync(CreateDeliverableCommand command);
        Task UpdateAsync(UpdateDeliverableCommand command);
        Task DeleteAsync(Guid id);
    }
}
