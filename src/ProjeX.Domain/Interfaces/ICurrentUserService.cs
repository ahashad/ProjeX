using ProjeX.Domain.Entities;

namespace ProjeX.Domain.Interfaces
{
    public interface ICurrentUserService
    {
        Task<ApplicationUser?> GetCurrentUserAsync();
    }
}