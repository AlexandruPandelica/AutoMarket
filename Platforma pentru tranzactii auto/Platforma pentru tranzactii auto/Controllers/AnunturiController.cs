using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;
using Microsoft.AspNetCore.Identity; // NECESAR
// Adaugam acest namespace pentru manipularea fisierelor
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Platforma_pentru_tranzactii_auto.Views
{
    public class AnunturiController : Controller
    {
        private readonly PlatformaDbContext _context;
        private readonly UserManager<Utilizator> _userManager;

        public AnunturiController(PlatformaDbContext context, UserManager<Utilizator> userManager)
        {
            _context = context;

            // 3. Aici facem legătura corectă
            // Luăm "userManager" primit ca parametru și îl punem în variabila clasei "_userManager"
            _userManager = userManager;
        }

        // GET: Anunturi
        // Adaugam parametrul "searchString"
        public async Task<IActionResult> Index(string searchString)
        {
            var anunturiQuery = _context.Anunt
                .Include(a => a.User)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();

                anunturiQuery = anunturiQuery.Where(s =>
                    s.Marca.ToLower().Contains(searchString) ||
                    s.Model.ToLower().Contains(searchString) ||
                    (s.Descriere != null && s.Descriere.ToLower().Contains(searchString)) ||
                    s.Locatie.ToLower().Contains(searchString)
                );
            }

            ViewData["CurrentFilter"] = searchString;

            // 🔥🔥🔥 CODUL NOU — se adaugă AICI 🔥🔥🔥
            if (User.Identity.IsAuthenticated)
            {
                int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

                var favoriteIds = await _context.Favorite
                    .Where(f => f.UserId == userId)
                    .Select(f => f.ID_Anunt)
                    .ToListAsync();

                ViewBag.FavoriteIds = favoriteIds;
            }
            else
            {
                ViewBag.FavoriteIds = new List<int>();
            }
            // 🔥🔥🔥 SFÂRȘIT COD NOU 🔥🔥🔥

            return View(await anunturiQuery.ToListAsync());
        }


        // GET: Anunturi/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var anunturi = await _context.Anunt
                .Include(a => a.User)
                // 2. IMPORTANT: Încărcăm comentariile și autorii lor
                .Include(a => a.Comentari)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.ID_Anunt == id);

            if (anunturi == null) return NotFound();

            // Incrementăm vizualizările
            anunturi.Nr_Vizualizari++;
            _context.Update(anunturi);
            await _context.SaveChangesAsync();

            return View(anunturi);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdaugaComentariu(int idAnunt, string textComentariu, int recenzie)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized(); // Sau un status code de eroare

            if (!string.IsNullOrWhiteSpace(textComentariu))
            {
                var comentariuNou = new Comentarii
                {
                    ID_Anunt = idAnunt,
                    UserId = user.Id,
                    User = user, // 🔥 IMPORTANT: Setăm userul ca să-i putem afișa email-ul imediat
                    Text_Comentariu = textComentariu,
                    Recenzie = recenzie,
                    DataComentariu = DateTime.UtcNow
                };

                _context.Comentarii.Add(comentariuNou);
                await _context.SaveChangesAsync();

                // 🔥 SCHIMBARE: Nu facem Redirect, ci returnăm HTML-ul comentariului nou
                return PartialView("_Comentariu", comentariuNou);
            }

            return BadRequest();
        }

        // GET: Anunturi/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id");
            return View();
        }

        // POST: Anunturi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Am adaugat "IFormFile? imagineUpload" in parametrii metodei
        public async Task<IActionResult> Create([Bind("ID_Anunt,Marca,Model,Pret,An_Fabricatie,Kilometraj,Descriere,Locatie,UserId")] Anunturi anunturi, IFormFile? imagineUpload)
        {
            // 1. Eliminam validarile automate care ne incurca
            ModelState.Remove("User");
            ModelState.Remove("Imagine_Anunt");

            // 2. Setam valorile automate
            anunturi.Data_Postarii = DateTime.UtcNow; // Folosim UTC pentru PostgreSQL
            anunturi.Nr_Vizualizari = 0;

            // 3. Procesam IMAGINEA
            if (imagineUpload != null && imagineUpload.Length > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await imagineUpload.CopyToAsync(memoryStream);
                    // Convertim in byte array si stocam in model
                    anunturi.Imagine_Anunt = memoryStream.ToArray();
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(anunturi);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Debugging: Daca validarea esueaza, poti vedea erorile aici
            // var errors = ModelState.Values.SelectMany(v => v.Errors);

            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id", anunturi.UserId);
            return View(anunturi);
        }

        // GET: Anunturi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var anunturi = await _context.Anunt.FindAsync(id);
            if (anunturi == null) return NotFound();

            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id", anunturi.UserId);
            return View(anunturi);
        }

        // POST: Anunturi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        // 1. Adaugam parametrul IFormFile? imagineUpload
        public async Task<IActionResult> Edit(int id, [Bind("ID_Anunt,Marca,Model,Pret,An_Fabricatie,Kilometraj,Descriere,Data_Postarii,Nr_Vizualizari,Locatie,UserId")] Anunturi anunturi, IFormFile? imagineUpload)
        {
            if (id != anunturi.ID_Anunt)
            {
                return NotFound();
            }

            ModelState.Remove("User");
            ModelState.Remove("Imagine_Anunt"); // Nu validam imaginea ca obligatorie

            // Fix pentru PostgreSQL - Data trebuie sa fie UTC
            anunturi.Data_Postarii = DateTime.SpecifyKind(anunturi.Data_Postarii, DateTimeKind.Utc);

            if (ModelState.IsValid)
            {
                try
                {
                    // 2. LOGICA CRITICĂ PENTRU IMAGINE
                    if (imagineUpload != null && imagineUpload.Length > 0)
                    {
                        // CAZUL A: Utilizatorul a încărcat o poză nouă -> O înlocuim pe cea veche
                        using (var memoryStream = new MemoryStream())
                        {
                            await imagineUpload.CopyToAsync(memoryStream);
                            anunturi.Imagine_Anunt = memoryStream.ToArray();
                        }
                    }
                    else
                    {
                        // CAZUL B: Utilizatorul NU a încărcat nimic -> Păstrăm poza veche
                        // Trebuie să citim din baza de date cum era anunțul înainte
                        // Folosim AsNoTracking() pentru a evita conflictele de tracking cu _context.Update
                        var anuntVechi = await _context.Anunt
                                                       .AsNoTracking()
                                                       .FirstOrDefaultAsync(a => a.ID_Anunt == id);

                        if (anuntVechi != null)
                        {
                            // Copiem imaginea veche în obiectul nou
                            anunturi.Imagine_Anunt = anuntVechi.Imagine_Anunt;
                        }
                    }

                    _context.Update(anunturi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnunturiExists(anunturi.ID_Anunt))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id", anunturi.UserId);
            return View(anunturi);
        }

        // GET: Anunturi/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var anunturi = await _context.Anunt
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.ID_Anunt == id);

            if (anunturi == null) return NotFound();

            return View(anunturi);
        }

        // POST: Anunturi/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anunturi = await _context.Anunt.FindAsync(id);
            if (anunturi != null)
            {
                _context.Anunt.Remove(anunturi);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AnunturiExists(int id)
        {
            return _context.Anunt.Any(e => e.ID_Anunt == id);
        }

        // POST: StergeComentariu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StergeComentariu(int id)
        {
            var comentariu = await _context.Comentarii.FindAsync(id);

            if (comentariu == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || comentariu.UserId != user.Id) return Forbid();

            _context.Comentarii.Remove(comentariu);
            await _context.SaveChangesAsync();

            // 🔥 MODIFICARE: Returnăm un status 200 OK, nu Redirect
            return Ok();
        }

    }
}