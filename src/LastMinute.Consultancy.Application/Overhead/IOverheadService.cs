using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LastMinute.Consultancy.Application.Overhead.Commands;

namespace LastMinute.Consultancy.Application.Overhead
{
    public interface IOverheadService
    {
        Task<List<OverheadDto>> GetAllAsync();
        Task<OverheadDto?> GetByIdAsync(Guid id);
        Task<OverheadDto> CreateAsync(CreateOverheadCommand command);
        Task UpdateAsync(UpdateOverheadCommand command);
        Task DeleteAsync(Guid id);
    }
}
