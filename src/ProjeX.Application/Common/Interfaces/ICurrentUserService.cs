using ProjeX.Domain.Entities;

namespace ProjeX.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        Task<ApplicationUser> GetCurrentUserAsync();
    }
}
