using System;
using LastMinute.Consultancy.Domain.Enums;

namespace LastMinute.Consultancy.Application.Client.Commands
{
    public class UpdateClientCommand
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public ClientStatus Status { get; set; }
    }
}


