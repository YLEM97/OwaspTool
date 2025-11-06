using AuthLibrary.Areas.Auth.Models;
using BytexDigital.Blazor.Components.CookieConsent;
using BytexDigital.Blazor.Components.CookieConsent.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace OwaspTool.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly HttpContextCookieConsent _cookieConsent;

        public LoginModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, HttpContextCookieConsent cookieConsent)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cookieConsent = cookieConsent;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        // single place for messages shown in the view
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public bool IsLoggedIn { get; set; }
        public bool PersistentCookiesAllowed { get; set; }
        public string? LoggedUserDisplayName { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? error = null, string? passwordchanged = null)
        {
            // if user already authenticated show message instead of form
            IsLoggedIn = User?.Identity?.IsAuthenticated == true;
            if (IsLoggedIn)
            {
                // try claims then fallback to identity name/email
                LoggedUserDisplayName =
                    User.FindFirst(ClaimTypes.Name)?.Value
                    ?? User.FindFirst("Name")?.Value
                    ?? User.Identity?.Name
                    ?? User.FindFirst(ClaimTypes.Email)?.Value
                    ?? "user";
            }

            if (passwordchanged == "1")
            {
                SuccessMessage = "Password changed successfully. Please log in with your new password.";
            }

            if (!string.IsNullOrEmpty(error))
            {
                ErrorMessage = error switch
                {
                    "invalid" => "Please check if email and/or password are correct.",
                    "confirmemail" => "Please confirm your email before logging in.",
                    "invalid_user" => "Please check if email and/or password are correct.",
                    _ => error
                };
            }

            PersistentCookiesAllowed = _cookieConsent
            .GetCookieConsentPreferences()?
            .IsCategoryAllowed("persistent-cookies") ?? false;

            await Task.CompletedTask;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = "/")
        {
            PersistentCookiesAllowed = _cookieConsent
                .GetCookieConsentPreferences()?
                .IsCategoryAllowed("persistent-cookies") ?? false;

            // keep server-side validation
            if (!ModelState.IsValid)
            {
                // let validation messages show for individual fields, but also set the generic error
                ErrorMessage ??= "Please check if email and/or password are correct.";
                Input.Email = string.Empty;
                ModelState.Remove("Input.Email");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                ErrorMessage = "Please check if email and/or password are correct.";
                Input.Email = string.Empty;
                ModelState.Remove("Input.Email");
                return Page();
            }

            if (!user.EmailConfirmed)
            {
                ErrorMessage = "Please confirm your email before logging in.";
                Input.Email = string.Empty;
                ModelState.Remove("Input.Email");
                return Page();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, Input.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                ErrorMessage = "Please check if email and/or password are correct.";
                Input.Email = string.Empty;
                ModelState.Remove("Input.Email");
                return Page();
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.Name ?? string.Empty),
                new("Name", user.Name ?? string.Empty),
                new("Surname", user.Surname ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Role, roles.FirstOrDefault() ?? "User")
            };

            await _signInManager.SignInWithClaimsAsync(user, Input.RememberMe, claims);

            return LocalRedirect(returnUrl ?? "/");
        }
    }
}