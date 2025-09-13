using ProjeX.Domain.Common;
using ProjeX.Domain.Enums;

namespace ProjeX.Domain.Entities
{
    public class Account : AuditableEntity
    {
        public Guid Id { get; set; }
        public string AccountNumber { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public AccountStatus Status { get; set; }
        public string Industry { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string BillingAddress { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public decimal AnnualRevenue { get; set; }
        public int NumberOfEmployees { get; set; }
        public Guid? AccountManagerId { get; set; }
        public DateTime? LastContactDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public virtual Employee? AccountManager { get; set; }
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public virtual ICollection<Opportunity> Opportunities { get; set; } = new List<Opportunity>();
        public virtual ICollection<Project> Projects { get; set; } = new List<Project>();
        public virtual ICollection<Tender> Tenders { get; set; } = new List<Tender>();
    }
}

