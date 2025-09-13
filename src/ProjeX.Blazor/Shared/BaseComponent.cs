using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ProjeX.Application.Identity;
using ProjeX.Domain.Entities;

namespace ProjeX.Blazor.Shared
{
    public class BaseComponent : ComponentBase
    {
        [CascadingParameter]
        protected Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

        [Inject]
        protected ICurrentUserService CurrentUserService { get; set; } = default!;

        protected ApplicationUser? CurrentUser { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            var authState = await AuthenticationStateTask;
            if (authState.User.Identity?.IsAuthenticated == true)
            {
                CurrentUser = await CurrentUserService.GetCurrentUserAsync();
            }
        }
    }
}
