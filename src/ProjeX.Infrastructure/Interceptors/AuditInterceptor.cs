using ProjeX.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ProjeX.Infrastructure.Interceptors
{
    public class AuditInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            UpdateAuditFields(eventData);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            UpdateAuditFields(eventData);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void UpdateAuditFields(DbContextEventData eventData)
        {
            if (eventData.Context == null) return;

            var currentUser = GetCurrentUser();

            foreach (var entry in eventData.Context.ChangeTracker.Entries<AuditableEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.CreatedBy = currentUser;
                    entry.Entity.ModifiedBy = currentUser; // Set ModifiedBy for new entities
                    entry.Entity.ModifiedAt = DateTime.UtcNow; // Set ModifiedAt for new entities
                }

                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = currentUser;
                }
            }
        }

        private string GetCurrentUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                return user.FindFirst(ClaimTypes.Name)?.Value 
                    ?? user.FindFirst(ClaimTypes.Email)?.Value 
                    ?? user.Identity.Name 
                    ?? "System";
            }
            return "System";
        }
    }
}


