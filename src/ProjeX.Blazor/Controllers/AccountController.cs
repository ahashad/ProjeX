using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LastMinute.Consultancy.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace ProjeX.Blazor.Controllers
{
    [ApiController]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
     SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [HttpPost("api/account/login")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Redirect("/Account/Login?error=Invalid input");
            }

            try
            {
                var result = await _signInManager.PasswordSignInAsync(
                         request.Email,
                  request.Password,
               request.RememberMe,
                       lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return Redirect("/");
                }
                else if (result.IsLockedOut)
                {
                    return Redirect("/Account/Login?error=Account is locked out");
                }
                else if (result.IsNotAllowed)
                {
                    return Redirect("/Account/Login?error=Account is not allowed to sign in");
                }
                else
                {
                    return Redirect("/Account/Login?error=Invalid email or password");
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Login error: {ex.Message}");
                return Redirect("/Account/Login?error=An error occurred during sign in");
            }
        }

        [HttpPost("api/account/logout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Redirect("/");
        }
    }

    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }
}