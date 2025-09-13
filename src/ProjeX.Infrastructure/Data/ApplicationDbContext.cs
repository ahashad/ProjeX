using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjeX.Domain.Entities;
using ProjeX.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace ProjeX.Infrastructure.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
   : base(options)
        {
        }

        public DbSet<RolesCatalog> RolesCatalogs { get; set; }
      public DbSet<Employee> Employees { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Project> Projects { get; set; }
 public DbSet<PlannedTeamSlot> PlannedTeamSlots { get; set; }
        public DbSet<Domain.Entities.Overhead> Overheads { get; set; }
        public DbSet<Deliverable> Deliverables { get; set; }
        public DbSet<ActualAssignment> ActualAssignments { get; set; }
        public DbSet<UserProjectClaim> UserProjectClaims { get; set; }
     public DbSet<TimeEntry> TimeEntries { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLineItem> InvoiceLineItems { get; set; }
 public DbSet<Payment> Payments { get; set; }
        public DbSet<ChangeRequest> ChangeRequests { get; set; }

        // Additional DbSets that were missing
        public DbSet<Domain.Entities.Task> Tasks { get; set; }
        public DbSet<TaskDependency> TaskDependencies { get; set; }
 public DbSet<Account> Accounts { get; set; }
   public DbSet<Opportunity> Opportunities { get; set; }
        public DbSet<OpportunityActivity> OpportunityActivities { get; set; }
        public DbSet<Tender> Tenders { get; set; }
      public DbSet<TenderActivity> TenderActivities { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorInvoice> VendorInvoices { get; set; }
      public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<InvoicePlan> InvoicePlans { get; set; }
        public DbSet<InvoiceSchedule> InvoiceSchedules { get; set; }
    public DbSet<Domain.Entities.Path> Paths { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<RolesCatalog>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Client>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Project>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<PlannedTeamSlot>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Domain.Entities.Overhead>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Deliverable>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<ActualAssignment>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<UserProjectClaim>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<TimeEntry>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Invoice>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<InvoiceLineItem>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<Payment>().HasQueryFilter(e => !e.IsDeleted);
            builder.Entity<ChangeRequest>().HasQueryFilter(e => !e.IsDeleted);

            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType))
                {
                    builder.Entity(entityType.ClrType).Property<byte[]>("RowVersion").IsRowVersion();
                }
            }

            builder.Entity<RolesCatalog>().Property(rc => rc.RoleName).HasMaxLength(256).IsRequired();
            builder.Entity<RolesCatalog>().Property(rc => rc.Notes).HasColumnType("TEXT");

            // Configure Employee entity
            builder.Entity<Employee>().Ignore(e => e.FullName);
            builder.Entity<Employee>().Ignore(e => e.Status);
            builder.Entity<Employee>().Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            builder.Entity<Employee>().Property(e => e.LastName).HasMaxLength(100).IsRequired();
            builder.Entity<Employee>().Property(e => e.Email).HasMaxLength(256).IsRequired();
            builder.Entity<Employee>().Property(e => e.Phone).HasMaxLength(20);
            builder.Entity<Employee>().Property(e => e.Salary).HasColumnType("decimal(18,2)");
            builder.Entity<Employee>().Property(e => e.MonthlyIncentive).HasColumnType("decimal(18,2)");
            builder.Entity<Employee>().Property(e => e.CommissionPercent).HasColumnType("decimal(5,2)");
            builder.Entity<Employee>().HasOne(e => e.Role).WithMany().HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Client>().Property(c => c.ClientName).HasMaxLength(256).IsRequired();
            builder.Entity<Client>().Property(c => c.ContactPerson).HasMaxLength(256).IsRequired();
            builder.Entity<Client>().Property(c => c.Email).HasMaxLength(256).IsRequired();
            builder.Entity<Client>().Property(c => c.Phone).HasMaxLength(20).IsRequired();
            builder.Entity<Client>().Property(c => c.Address).HasMaxLength(500).IsRequired();

            // Configure Project entity
            builder.Entity<Project>().Property(p => p.ProjectName).HasMaxLength(256).IsRequired();
            builder.Entity<Project>().Property(p => p.Budget).HasColumnType("decimal(18,2)");
            builder.Entity<Project>().Property(p => p.ProjectPrice).HasColumnType("decimal(18,2)");
            builder.Entity<Project>().Property(p => p.ExpectedWorkingPeriodMonths).HasColumnType("decimal(8,2)");
            builder.Entity<Project>().Property(p => p.Notes).HasColumnType("TEXT");
            builder.Entity<Project>().HasOne(p => p.Client).WithMany().HasForeignKey(p => p.ClientId).OnDelete(DeleteBehavior.Restrict);

            // Configure PlannedTeamSlot entity
            builder.Entity<PlannedTeamSlot>().Property(pts => pts.PeriodMonths).HasColumnType("decimal(8,2)");
            builder.Entity<PlannedTeamSlot>().Property(pts => pts.AllocationPercent).HasColumnType("decimal(5,2)");
            builder.Entity<PlannedTeamSlot>().Property(pts => pts.PlannedSalary).HasColumnType("decimal(18,2)");
            builder.Entity<PlannedTeamSlot>().Property(pts => pts.PlannedIncentive).HasColumnType("decimal(18,2)");
            builder.Entity<PlannedTeamSlot>().Property(pts => pts.PlannedCommissionPercent).HasColumnType("decimal(5,2)");
            builder.Entity<PlannedTeamSlot>().Property(pts => pts.ComputedBudgetCost).HasColumnType("decimal(18,2)");
            builder.Entity<PlannedTeamSlot>().HasOne(pts => pts.Project).WithMany().HasForeignKey(pts => pts.ProjectId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PlannedTeamSlot>().HasOne(pts => pts.Role).WithMany().HasForeignKey(pts => pts.RoleId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<PlannedTeamSlot>().HasIndex(pts => new { pts.ProjectId, pts.RoleId }).HasDatabaseName("IX_PlannedTeamSlot_Project_Role");

            // Configure Identity tables for SQLite
            builder.Entity<ApplicationUser>(b =>
            {
                b.Property(u => u.UserName).HasMaxLength(256);
                b.Property(u => u.NormalizedUserName).HasMaxLength(256);
                b.Property(u => u.Email).HasMaxLength(256);
                b.Property(u => u.NormalizedEmail).HasMaxLength(256);
            });

            builder.Entity<IdentityRole>(b =>
            {
                b.Property(r => r.Name).HasMaxLength(256);
                b.Property(r => r.NormalizedName).HasMaxLength(256);
            });

            builder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.Property(l => l.LoginProvider).HasMaxLength(128);
                b.Property(l => l.ProviderKey).HasMaxLength(128);
            });

            builder.Entity<IdentityUserToken<string>>(b =>
            {
                b.Property(t => t.LoginProvider).HasMaxLength(128);
                b.Property(t => t.Name).HasMaxLength(128);
            });

            // Configure Overhead entity
            builder.Entity<Domain.Entities.Overhead>().Property(o => o.Description).HasMaxLength(500).IsRequired();
            builder.Entity<Domain.Entities.Overhead>().Property(o => o.Amount).HasColumnType("decimal(18,2)");
            builder.Entity<Domain.Entities.Overhead>().HasOne(o => o.Project).WithMany().HasForeignKey(o => o.ProjectId).OnDelete(DeleteBehavior.Restrict);

            // Configure Deliverable entity
            builder.Entity<Deliverable>().Property(d => d.Name).HasMaxLength(256).IsRequired();
            builder.Entity<Deliverable>().Property(d => d.Description).HasMaxLength(1000).IsRequired();
            builder.Entity<Deliverable>().HasOne(d => d.Project).WithMany().HasForeignKey(d => d.ProjectId).OnDelete(DeleteBehavior.Restrict);

            // Configure ActualAssignment entity
            builder.Entity<ActualAssignment>().Property(aa => aa.AllocationPercent).HasColumnType("decimal(5,2)");
            builder.Entity<ActualAssignment>().Property(aa => aa.CostDifferenceAmount).HasColumnType("decimal(18,2)");
            builder.Entity<ActualAssignment>().Property(aa => aa.StartDate).IsRequired();
            builder.Entity<ActualAssignment>().Property(aa => aa.EndDate);
            builder.Entity<ActualAssignment>().HasOne(aa => aa.Project).WithMany().HasForeignKey(aa => aa.ProjectId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ActualAssignment>().HasOne(aa => aa.PlannedTeamSlot).WithMany().HasForeignKey(aa => aa.PlannedTeamSlotId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ActualAssignment>().HasOne(aa => aa.Employee).WithMany().HasForeignKey(aa => aa.EmployeeId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ActualAssignment>().HasIndex(aa => new { aa.ProjectId, aa.PlannedTeamSlotId }).HasDatabaseName("IX_ActualAssignment_Project_PlannedSlot");
            builder.Entity<ActualAssignment>().HasIndex(aa => new { aa.EmployeeId, aa.Status }).HasDatabaseName("IX_ActualAssignment_Employee_Status");
            builder.Entity<ActualAssignment>().HasIndex(aa => new { aa.EmployeeId, aa.StartDate, aa.EndDate }).HasDatabaseName("IX_ActualAssignment_Employee_DateRange");

            // Configure UserProjectClaim entity
            builder.Entity<UserProjectClaim>().Property(upc => upc.UserId).HasMaxLength(450).IsRequired();
            builder.Entity<UserProjectClaim>().Property(upc => upc.ClaimType).HasMaxLength(256).IsRequired();
            builder.Entity<UserProjectClaim>().Property(upc => upc.ClaimValue).HasMaxLength(256).IsRequired();
            builder.Entity<UserProjectClaim>().HasOne(upc => upc.Project).WithMany().HasForeignKey(upc => upc.ProjectId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<UserProjectClaim>().HasIndex(upc => new { upc.UserId, upc.ProjectId, upc.ClaimType }).HasDatabaseName("IX_UserProjectClaim_User_Project_Claim").IsUnique();

            // Configure RolesCatalog decimal properties
            builder.Entity<RolesCatalog>().Property(rc => rc.DefaultSalary).HasColumnType("decimal(18,2)");
            builder.Entity<RolesCatalog>().Property(rc => rc.DefaultMonthlyIncentive).HasColumnType("decimal(18,2)");
            builder.Entity<RolesCatalog>().Property(rc => rc.CommissionPercent).HasColumnType("decimal(5,2)");

            // Configure TimeEntry entity
            builder.Entity<TimeEntry>().Property(te => te.Hours).HasColumnType("decimal(8,2)");
            builder.Entity<TimeEntry>().Property(te => te.BillableRate).HasColumnType("decimal(18,2)");
            builder.Entity<TimeEntry>().Property(te => te.Description).HasMaxLength(1000);
            builder.Entity<TimeEntry>().HasOne(te => te.ActualAssignment).WithMany(aa => aa.TimeEntries).HasForeignKey(te => te.ActualAssignmentId).OnDelete(DeleteBehavior.Restrict);

            // Configure Invoice entity
            builder.Entity<Invoice>().Property(i => i.InvoiceNumber).HasMaxLength(100).IsRequired();
            builder.Entity<Invoice>().Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
            builder.Entity<Invoice>().Property(i => i.SubTotal).HasColumnType("decimal(18,2)");
            builder.Entity<Invoice>().Property(i => i.TaxAmount).HasColumnType("decimal(18,2)");
            builder.Entity<Invoice>().Property(i => i.Notes).HasColumnType("TEXT");
            builder.Entity<Invoice>().HasOne(i => i.Project).WithMany().HasForeignKey(i => i.ProjectId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Invoice>().HasOne(i => i.Client).WithMany().HasForeignKey(i => i.ClientId).OnDelete(DeleteBehavior.Restrict);

            // Configure InvoiceLineItem entity
            builder.Entity<InvoiceLineItem>().Property(ili => ili.Description).HasMaxLength(500).IsRequired();
            builder.Entity<InvoiceLineItem>().Property(ili => ili.Quantity).HasColumnType("decimal(10,2)");
            builder.Entity<InvoiceLineItem>().Property(ili => ili.UnitRate).HasColumnType("decimal(18,2)");
            builder.Entity<InvoiceLineItem>().Property(ili => ili.Amount).HasColumnType("decimal(18,2)");
            builder.Entity<InvoiceLineItem>().HasOne(ili => ili.Invoice).WithMany(i => i.LineItems).HasForeignKey(ili => ili.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<InvoiceLineItem>().HasOne(ili => ili.TimeEntry).WithMany().HasForeignKey(ili => ili.TimeEntryId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<InvoiceLineItem>().HasOne(ili => ili.Overhead).WithMany().HasForeignKey(ili => ili.OverheadId).OnDelete(DeleteBehavior.Restrict);

            // Configure Payment entity
            builder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
            builder.Entity<Payment>().Property(p => p.Reference).HasMaxLength(100);
            builder.Entity<Payment>().Property(p => p.Notes).HasColumnType("TEXT");
            builder.Entity<Payment>().HasOne(p => p.Invoice).WithMany(i => i.Payments).HasForeignKey(p => p.InvoiceId).OnDelete(DeleteBehavior.Restrict);

            // Configure ChangeRequest entity
            builder.Entity<ChangeRequest>().Property(cr => cr.Title).HasMaxLength(256).IsRequired();
            builder.Entity<ChangeRequest>().Property(cr => cr.Description).HasColumnType("TEXT").IsRequired();
            builder.Entity<ChangeRequest>().Property(cr => cr.BusinessJustification).HasColumnType("TEXT").IsRequired();
            builder.Entity<ChangeRequest>().Property(cr => cr.EstimatedCost).HasColumnType("decimal(18,2)");
            builder.Entity<ChangeRequest>().Property(cr => cr.EstimatedHours).HasColumnType("decimal(8,2)");
            builder.Entity<ChangeRequest>().Property(cr => cr.ActualCost).HasColumnType("decimal(18,2)");
            builder.Entity<ChangeRequest>().Property(cr => cr.ActualHours).HasColumnType("decimal(8,2)");
            builder.Entity<ChangeRequest>().Property(cr => cr.RequestNumber).HasMaxLength(50).IsRequired();
            builder.Entity<ChangeRequest>().Property(cr => cr.RequestedBy).HasMaxLength(256).IsRequired();
            builder.Entity<ChangeRequest>().Property(cr => cr.ReviewComments).HasColumnType("TEXT");
            builder.Entity<ChangeRequest>().Property(cr => cr.ApprovalComments).HasColumnType("TEXT");
            builder.Entity<ChangeRequest>().HasOne(cr => cr.Project).WithMany().HasForeignKey(cr => cr.ProjectId).OnDelete(DeleteBehavior.Restrict);

            // TODO: Configure TaskDependency entity relationships properly
            // Configure TaskDependency entity
            builder.Entity<TaskDependency>()
                .HasKey(td => new { td.TaskId, td.DependentTaskId });

            builder.Entity<TaskDependency>()
                .HasOne(td => td.Task)
                .WithMany()
                .HasForeignKey(td => td.TaskId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<TaskDependency>()
                .HasOne(td => td.DependentTask)
                .WithMany()
                .HasForeignKey(td => td.DependentTaskId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure GoodsReceiptItem to fix cascade delete conflict
            builder.Entity<GoodsReceiptItem>()
                .HasOne(gri => gri.GoodsReceipt)
                .WithMany()
                .HasForeignKey(gri => gri.GoodsReceiptId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<GoodsReceiptItem>()
                .HasOne(gri => gri.PurchaseOrderItem)
                .WithMany()
                .HasForeignKey(gri => gri.PurchaseOrderItemId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}


