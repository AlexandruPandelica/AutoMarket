using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Platforma_pentru_tranzactii_auto.Models;

namespace Platforma_pentru_tranzactii_auto.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<Utilizator> _userManager;
        private readonly SignInManager<Utilizator> _signInManager;

        public IndexModel(
            UserManager<Utilizator> userManager,
            SignInManager<Utilizator> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        // Va conține imaginea Base64 pentru afișare
        public string UserImage { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Nume")]
            public string Nume { get; set; }

            [Display(Name = "Prenume")]
            public string Prenume { get; set; }

            [Phone]
            [Display(Name = "Telefon")]
            public string Telefon { get; set; }

            [Display(Name = "Adresa")]
            public string Adresa { get; set; }

            [Display(Name = "Schimbă poza de profil")]
            public IFormFile ImagineProfilFile { get; set; }
        }

        private async Task LoadAsync(Utilizator user)
        {
            Username = await _userManager.GetUserNameAsync(user);

            // Imagine Base64
            if (user.Imagine_Profil != null && user.Imagine_Profil.Length > 0)
            {
                UserImage = $"data:image/png;base64,{Convert.ToBase64String(user.Imagine_Profil)}";
            }
            else
            {
                UserImage = "https://cdn.icon-icons.com/icons2/2506/PNG/512/user_icon_150670.png";
            }

            Input = new InputModel
            {
                Nume = user.Nume,
                Prenume = user.Prenume,
                Telefon = user.Telefon,
                Adresa = user.Adresa
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            // Update text fields
            user.Nume = Input.Nume;
            user.Prenume = Input.Prenume;
            user.Telefon = Input.Telefon;
            user.Adresa = Input.Adresa;

            // Update imagine
            if (Input.ImagineProfilFile != null)
            {
                using (var ms = new MemoryStream())
                {
                    await Input.ImagineProfilFile.CopyToAsync(ms);
                    user.Imagine_Profil = ms.ToArray();
                }
            }

            // Salvăm modificările
            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Profilul a fost actualizat cu succes!";
            return RedirectToPage();
        }
    }
}
