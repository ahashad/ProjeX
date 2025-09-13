using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using ProjeX.Application.Identity;
using ProjeX.Domain.Entities;
using System.Security.Claims;

namespace ProjeX.Infrastructure.Services
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

        public async Task<ApplicationUser?> GetCurrentUserAsync()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }

            return await _userManager.FindByIdAsync(userId);
        }
    }
}
