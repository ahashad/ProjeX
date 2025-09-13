using ProjeX.Domain.Enums;

namespace ProjeX.Application.Client
{
    public class ClientDto
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ContactPerson { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public ClientStatus Status { get; set; }
        
        // Audit fields
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public DateTime? ModifiedAt { get; set; }
    }
}


