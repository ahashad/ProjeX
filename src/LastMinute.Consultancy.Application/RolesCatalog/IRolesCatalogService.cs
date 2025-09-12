using LastMinute.Consultancy.Application.RolesCatalog.Commands;

namespace LastMinute.Consultancy.Application.RolesCatalog
{
    public interface IRolesCatalogService
    {
        Task<List<RolesCatalogDto>> GetAllAsync();
        Task<RolesCatalogDto?> GetByIdAsync(Guid id);
        Task<RolesCatalogDto> CreateAsync(CreateRolesCatalogCommand command);
        Task UpdateAsync(UpdateRolesCatalogCommand command);
        Task DeleteAsync(Guid id);
    }
}
