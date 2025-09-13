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
        Task<ClientDto> CreateAsync(CreateClientCommand command, string userId);
       Task UpdateAsync(UpdateClientCommand command, string userId);
        Task DeleteAsync(Guid id);
    }
}

