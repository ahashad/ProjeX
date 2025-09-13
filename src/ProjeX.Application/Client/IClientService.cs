using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjeX.Application.Client.Commands;

namespace ProjeX.Application.Client
{
    public interface IClientService
    {
        Task<List<ClientDto>> GetAllAsync();
        Task<ClientDto?> GetByIdAsync(Guid id);
        Task<ClientDto> CreateAsync(CreateClientCommand command);
        Task UpdateAsync(UpdateClientCommand command);
        Task DeleteAsync(Guid id);
    }
}

