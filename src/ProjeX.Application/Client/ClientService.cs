using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using ProjeX.Application.Client.Commands;
using LastMinute.Consultancy.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ProjeX.Application.Client
{
    public class ClientService : IClientService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ClientService(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<ClientDto>> GetAllAsync()
        {
            var clients = await _context.Clients.ToListAsync();
            return _mapper.Map<List<ClientDto>>(clients);
        }

        public async Task<ClientDto?> GetByIdAsync(Guid id)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(c => c.Id == id);
            return client != null ? _mapper.Map<ClientDto>(client) : null;
        }

        public async Task<ClientDto> CreateAsync(CreateClientCommand command)
        {
            var entity = new Domain.Entities.Client
            {
                Id = Guid.NewGuid(),
                ClientName = command.ClientName,
                ContactPerson = command.ContactPerson,
                Email = command.Email,
                Phone = command.Phone,
                Address = command.Address,
                Status = command.Status
            };

            _context.Clients.Add(entity);
            await _context.SaveChangesAsync();

            return _mapper.Map<ClientDto>(entity);
        }

        public async Task UpdateAsync(UpdateClientCommand command)
        {
            var entity = await _context.Clients.FindAsync(command.Id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Client with ID {command.Id} not found.");
            }

            entity.ClientName = command.ClientName;
            entity.ContactPerson = command.ContactPerson;
            entity.Email = command.Email;
            entity.Phone = command.Phone;
            entity.Address = command.Address;
            entity.Status = command.Status;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var entity = await _context.Clients.FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Client with ID {id} not found.");
            }

            _context.Clients.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}

