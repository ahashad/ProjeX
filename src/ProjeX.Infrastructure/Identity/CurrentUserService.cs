using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ProjeX.Application.Common.Interfaces;
using ProjeX.Domain.Entities;

namespace ProjeX.Infrastructure.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApplicationUser> GetCurrentUserAsync()
        {
            var principal = _httpContextAccessor.HttpContext?.User;
            return await _userManager.GetUserAsync(principal!)!;
        }
    }
}
