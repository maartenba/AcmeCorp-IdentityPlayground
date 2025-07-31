using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AcmeCorp.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AcmeCorp.IdentityServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterPasskeyModel : PageModel
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<RegisterPasskeyModel> _logger;

    public RegisterPasskeyModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<RegisterPasskeyModel> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        public string Name { get; set; }
            
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        public PasskeyInputModel? Passkey { get; set; }
    }

    public IActionResult OnGetAsync()
    {
        return RedirectToPage("/Login");
    }

    public IActionResult OnPost(string? returnUrl = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        
        ReturnUrl = returnUrl;

        if (!string.IsNullOrEmpty(Input?.Passkey?.Error))
        {
            ErrorMessage = $"Could not add a passkey: {Input.Passkey.Error}";
            return Page();
        }

        if (string.IsNullOrEmpty(Input?.Passkey?.CredentialJson))
        {
            ErrorMessage = "The browser did not provide a passkey.";
            return Page();
        }
        
        // Ask the user to create an account.
        ModelState.Clear();
        return Page();
    }

    public async Task<IActionResult> OnPostConfirmationAsync(string returnUrl = null)
    {
        returnUrl = returnUrl ?? Url.Content("~/");
        
        ReturnUrl = returnUrl;

        if (!string.IsNullOrEmpty(Input?.Passkey?.Error))
        {
            ErrorMessage = $"Could not add a passkey: {Input.Passkey.Error}";
            return Page();
        }

        if (string.IsNullOrEmpty(Input?.Passkey?.CredentialJson))
        {
            ErrorMessage = "The browser did not provide a passkey.";
            return Page();
        }
        
        if (ModelState.IsValid)
        {
            var passkeyAuthenticateResult = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
            
            var username = passkeyAuthenticateResult.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                           ?? throw new InvalidOperationException("Unknown userid");
            var user = new ApplicationUser { Id = username /* <-- important! */, UserName = username, Email = Input.Email };
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                var attestationResult = await _signInManager.PerformPasskeyAttestationAsync(Input.Passkey.CredentialJson);
                if (!attestationResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);
                    ErrorMessage = $"Could not add the passkey: {attestationResult.Failure.Message}.";
                    return Page();
                }
                
                result = await _userManager.AddOrUpdatePasskeyAsync(user, attestationResult.Passkey);
            }

            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
                        
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User created an account using passkey");
                return LocalRedirect(returnUrl);
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        
        return Page();
    }
}