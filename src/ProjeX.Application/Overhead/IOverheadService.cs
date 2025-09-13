using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjeX.Application.Overhead.Commands;

namespace ProjeX.Application.Overhead
{
    public interface IOverheadService
    {
        Task<List<OverheadDto>> GetAllAsync();
        Task<OverheadDto?> GetByIdAsync(Guid id);
        Task<OverheadDto> CreateAsync(CreateOverheadCommand command, string userId);
        Task UpdateAsync(UpdateOverheadCommand command, string userId);
        Task DeleteAsync(Guid id);
    }
}
