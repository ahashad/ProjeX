using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using ProjeX.Domain.Interfaces;
using ProjeX.Domain.Entities;

namespace ProjeX.Blazor.Shared
{
    public class BaseComponent : ComponentBase
    {
        [CascadingParameter]
        protected System.Threading.Tasks.Task<AuthenticationState> AuthenticationStateTask { get; set; } = default!;

        protected ApplicationUser? CurrentUser { get; private set; }

        protected override async System.Threading.Tasks.Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var authState = await AuthenticationStateTask;
                if (authState.User.Identity?.IsAuthenticated == true)
                {
                    // Derive user id from claims to avoid DbContext usage during render
                    var userId = authState.User.FindFirst("sub")?.Value
                                 ?? authState.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                 ?? authState.User.Identity?.Name
                                 ?? "System";
                    // Lightweight ApplicationUser with only Id for auditing needs
                    CurrentUser = new ApplicationUser { Id = userId };
                    StateHasChanged();
                }
            }
        }
    }
}
