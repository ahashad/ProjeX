using ProjeX.Domain.Entities;

namespace ProjeX.Application.Identity
{
    public interface ICurrentUserService
    {
        Task<ApplicationUser?> GetCurrentUserAsync();
    }
}
