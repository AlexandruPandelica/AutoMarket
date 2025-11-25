using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO; // Necesar pentru MemoryStream (imagine)
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
// Asigură-te că ai namespace-ul corect pentru modelele tale
using Platforma_pentru_tranzactii_auto.Models;

namespace Platforma_pentru_tranzactii_auto.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<Utilizator> _signInManager;
        private readonly UserManager<Utilizator> _userManager;
        private readonly IUserStore<Utilizator> _userStore;
        private readonly IUserEmailStore<Utilizator> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        // Injectăm RoleManager cu tipul <IdentityRole<int>>
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public RegisterModel(
            UserManager<Utilizator> userManager,
            IUserStore<Utilizator> userStore,
            SignInManager<Utilizator> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            // Aici este cheia: IdentityRole<int>
            RoleManager<IdentityRole<int>> roleManager)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(50, ErrorMessage = "Numele nu poate depăși {1} caractere.")]
            [Display(Name = "Nume")]
            public string Nume { get; set; }

            [Required]
            [StringLength(50, ErrorMessage = "Prenumele nu poate depăși {1} caractere.")]
            [Display(Name = "Prenume")]
            public string Prenume { get; set; }

            [Required]
            [Phone]
            [Display(Name = "Telefon")]
            public string Telefon { get; set; }

            [Required]
            [StringLength(200, ErrorMessage = "Adresa nu poate depăși {1} caractere.")]
            [Display(Name = "Adresa")]
            public string Adresa { get; set; }

            [Display(Name = "Imagine Profil")]
            public IFormFile ImagineProfilFile { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "Parola trebuie să aibă cel puțin {2} și maximum {1} caractere.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Parola")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmă parola")]
            [Compare("Password", ErrorMessage = "Parola și confirmarea parolei nu se potrivesc.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                var user = CreateUser();

                // Setăm datele standard Identity
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Setăm datele tale personalizate
                user.Nume = Input.Nume;
                user.Prenume = Input.Prenume;
                user.Telefon = Input.Telefon;
                user.Adresa = Input.Adresa;

                // --- LOGICĂ IMAGINE ---
                if (Input.ImagineProfilFile != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await Input.ImagineProfilFile.CopyToAsync(memoryStream);
                        user.Imagine_Profil = memoryStream.ToArray();
                    }
                }
                // ----------------------

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    // --- LOGICĂ ROLURI ---
                    // Aici decidem ce rol primește.
                    // Varianta A: Rol fix "Utilizator"
                    string roleName = "Client";

                    // Varianta B: Rol bazat pe ce a ales in formular (decomentează dacă vrei asta)

                    // Verificăm dacă rolul există în baza de date
                    var roleExists = await _roleManager.RoleExistsAsync(roleName);
                    if (!roleExists)
                    {
                        // Îl creăm folosind constructorul pentru IdentityRole<int>
                        await _roleManager.CreateAsync(new IdentityRole<int>(roleName));
                    }

                    // Adăugăm userul în rol
                    await _userManager.AddToRoleAsync(user, roleName);
                    // ---------------------

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Dacă am ajuns aici, ceva a eșuat, reafișăm formularul
            return Page();
        }

        private Utilizator CreateUser()
        {
            try
            {
                return Activator.CreateInstance<Utilizator>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(Utilizator)}'. " +
                    $"Ensure that '{nameof(Utilizator)}' is not an abstract class and has a parameterless constructor.");
            }
        }

        private IUserEmailStore<Utilizator> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<Utilizator>)_userStore;
        }
    }
}