using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;

namespace Platforma_pentru_tranzactii_auto.Controllers
{
    public class AnunturiController : Controller
    {
        private readonly PlatformaDbContext _context;
        private readonly UserManager<Utilizator> _userManager;
        private readonly IWebHostEnvironment _environment;

        public AnunturiController(PlatformaDbContext context, UserManager<Utilizator> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _environment = env;
        }

        // ======================================================
        // LISTĂ ANUNȚURI + FILTRARE
        // ======================================================
        public async Task<IActionResult> Index(string marca, string model, int? pretMin, int? pretMax)
        {
            var anunturi = _context.Anunt
                .Include(a => a.Imagine)
                .Include(a => a.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(marca))
                anunturi = anunturi.Where(a => a.Marca.Contains(marca));

            if (!string.IsNullOrEmpty(model))
                anunturi = anunturi.Where(a => a.Model.Contains(model));

            if (pretMin.HasValue)
                anunturi = anunturi.Where(a => a.Pret >= pretMin.Value);

            if (pretMax.HasValue)
                anunturi = anunturi.Where(a => a.Pret <= pretMax.Value);

            return View(await anunturi.ToListAsync());
        }

        // ======================================================
        // DETALII ANUNȚ + INCREMENTARE VIZUALIZĂRI
        // ======================================================
        public async Task<IActionResult> Details(int id)
        {
            var anunt = await _context.Anunt
                .Include(a => a.Imagine)
                .Include(a => a.User)
                .Include(a => a.Comentari)
                .FirstOrDefaultAsync(a => a.ID_Anunt == id);

            if (anunt == null)
                return NotFound();

            anunt.Nr_Vizualizari++;
            await _context.SaveChangesAsync();

            return View(anunt);
        }

        // ======================================================
        // CREATE
        // ======================================================
        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Anunturi anunt, List<IFormFile> poze)
        {
            if (!ModelState.IsValid)
                return View(anunt);

            var user = await _userManager.GetUserAsync(User);
            anunt.UserId = user.Id;
            anunt.Data_Postarii = DateTime.Now;

            _context.Anunt.Add(anunt);
            await _context.SaveChangesAsync(); // SALVĂM ANUNȚUL ÎNAINTE DE IMAGINI

            // DEBUG — verificăm dacă avem poze
            Console.WriteLine("Numar poze: " + (poze?.Count ?? 0));

            // Dacă sunt poze => le salvăm
            if (poze != null && poze.Count > 0)
            {
                foreach (var poza in poze)
                {
                    var folder = Path.Combine(_environment.WebRootPath, "imagini");
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(poza.FileName);
                    var filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await poza.CopyToAsync(stream);
                    }

                    var imagine = new ImagineMasina
                    {
                        ID_Anunt = anunt.ID_Anunt,
                        Cale_Imagine = "/imagini/" + fileName
                    };

                    _context.Imagine.Add(imagine);
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }


        // ======================================================
        // EDIT — doar dacă utilizatorul este proprietar
        // ======================================================
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            var anunt = await _context.Anunt
                .Include(a => a.Imagine)
                .FirstOrDefaultAsync(a => a.ID_Anunt == id);

            if (anunt == null)
                return NotFound();

            if (anunt.UserId != int.Parse(_userManager.GetUserId(User)))
                return Unauthorized();

            return View(anunt);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Edit(Anunturi anunt, List<IFormFile> noiPoze)
        {
            var original = await _context.Anunt.FindAsync(anunt.ID_Anunt);

            if (original == null)
                return NotFound();

            if (original.UserId != int.Parse(_userManager.GetUserId(User)))
                return Unauthorized();

            // Actualizare câmpuri
            original.Marca = anunt.Marca;
            original.Model = anunt.Model;
            original.Pret = anunt.Pret;
            original.Kilometraj = anunt.Kilometraj;
            original.An_Fabricatie = anunt.An_Fabricatie;
            original.Descriere = anunt.Descriere;
            original.Locatie = anunt.Locatie;

            await _context.SaveChangesAsync();

            // Upload noi poze
            if (noiPoze != null && noiPoze.Count > 0)
            {
                foreach (var poza in noiPoze)
                {
                    string folder = Path.Combine(_environment.WebRootPath, "imagini");
                    if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid() + Path.GetExtension(poza.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await poza.CopyToAsync(stream);

                    var imagine = new ImagineMasina
                    {
                        ID_Anunt = original.ID_Anunt,
                        Cale_Imagine = "/imagini/" + fileName
                    };

                    _context.Imagine.Add(imagine);
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ======================================================
        // DELETE — doar dacă utilizatorul este proprietar
        // ======================================================
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var anunt = await _context.Anunt
                .Include(a => a.Imagine)
                .FirstOrDefaultAsync(a => a.ID_Anunt == id);

            if (anunt == null)
                return NotFound();

            if (anunt.UserId != int.Parse(_userManager.GetUserId(User)))
                return Unauthorized();

            return View(anunt);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anunt = await _context.Anunt
                .Include(a => a.Imagine)
                .FirstOrDefaultAsync(a => a.ID_Anunt == id);

            if (anunt == null)
                return NotFound();

            // Ștergere poze din wwwroot
            if (anunt.Imagine != null)
            {
                foreach (var img in anunt.Imagine)
                {
                    string path = Path.Combine(_environment.WebRootPath, img.Cale_Imagine.TrimStart('/'));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
            }

            _context.Anunt.Remove(anunt);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
