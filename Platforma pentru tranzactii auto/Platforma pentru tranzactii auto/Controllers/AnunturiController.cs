using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;
using Microsoft.AspNetCore.Identity;
using System.IO;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Platforma_pentru_tranzactii_auto.Controllers // Notă: De obicei controller-ele stau în namespace-ul .Controllers
{
    public class AnunturiController : Controller
    {
        private readonly PlatformaDbContext _context;
        private readonly UserManager<Utilizator> _userManager;

        public AnunturiController(PlatformaDbContext context, UserManager<Utilizator> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Anunturi
        // 🔥 MODIFICARE: Am adăugat parametrii pentru filtre (minPret, maxPret, minAn, maxAn)
        public async Task<IActionResult> Index(string searchString, int? minPret, int? maxPret, int? minAn, int? maxAn)
        {
            var anunturiQuery = _context.Anunt
                .Include(a => a.User)
                .AsQueryable();

            // 1. Filtrare Text (Marcă, Model, Descriere, Locație)
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

            // 2. Filtrare PREȚ
            if (minPret.HasValue)
            {
                anunturiQuery = anunturiQuery.Where(s => s.Pret >= minPret.Value);
            }
            if (maxPret.HasValue)
            {
                anunturiQuery = anunturiQuery.Where(s => s.Pret <= maxPret.Value);
            }

            // 3. Filtrare AN FABRICAȚIE
            if (minAn.HasValue)
            {
                anunturiQuery = anunturiQuery.Where(s => s.An_Fabricatie >= minAn.Value);
            }
            if (maxAn.HasValue)
            {
                anunturiQuery = anunturiQuery.Where(s => s.An_Fabricatie <= maxAn.Value);
            }

            // 4. Salvăm valorile în ViewData pentru a le afișa înapoi în formular
            ViewData["CurrentFilter"] = searchString;
            ViewData["MinPret"] = minPret;
            ViewData["MaxPret"] = maxPret;
            ViewData["MinAn"] = minAn;
            ViewData["MaxAn"] = maxAn;

            // 5. Logica pentru Favorite
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

            return View(await anunturiQuery.ToListAsync());
        }


        // GET: Anunturi/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var anunturi = await _context.Anunt
                .Include(a => a.User)
                .Include(a => a.Comentari).ThenInclude(c => c.User)
                .Include(a => a.GalerieImagini)
                .FirstOrDefaultAsync(m => m.ID_Anunt == id);

            if (anunturi == null) return NotFound();

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
            if (user == null) return Unauthorized();

            if (!string.IsNullOrWhiteSpace(textComentariu))
            {
                var comentariuNou = new Comentarii
                {
                    ID_Anunt = idAnunt,
                    UserId = user.Id,
                    User = user,
                    Text_Comentariu = textComentariu,
                    Recenzie = recenzie,
                    DataComentariu = DateTime.UtcNow
                };

                _context.Comentarii.Add(comentariuNou);
                await _context.SaveChangesAsync();

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
        public async Task<IActionResult> Create([Bind("ID_Anunt,Marca,Model,Pret,An_Fabricatie,Kilometraj,Descriere,Locatie,UserId")] Anunturi anunturi, IEnumerable<IFormFile>? imaginiUpload)
        {
            ModelState.Remove("User");
            ModelState.Remove("Imagine_Anunt");
            ModelState.Remove("GalerieImagini");

            anunturi.Data_Postarii = DateTime.UtcNow;
            anunturi.Nr_Vizualizari = 0;

            if (imaginiUpload != null && imaginiUpload.Any())
            {
                var primaImagine = imaginiUpload.First();
                using (var memoryStream = new MemoryStream())
                {
                    await primaImagine.CopyToAsync(memoryStream);
                    anunturi.Imagine_Anunt = memoryStream.ToArray();
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(anunturi);
                await _context.SaveChangesAsync();

                if (imaginiUpload != null && imaginiUpload.Count() > 0)
                {
                    foreach (var img in imaginiUpload)
                    {
                        using (var stream = new MemoryStream())
                        {
                            await img.CopyToAsync(stream);
                            var imagineNoua = new ImaginiAnunt
                            {
                                ID_Anunt = anunturi.ID_Anunt,
                                Imagine = stream.ToArray()
                            };
                            _context.ImaginiAnunt.Add(imagineNoua);
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id", anunturi.UserId);
            return View(anunturi);
        }

        // GET: Anunturi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var anunturi = await _context.Anunt
                .Include(a => a.GalerieImagini)
                .FirstOrDefaultAsync(m => m.ID_Anunt == id);

            if (anunturi == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || anunturi.UserId != user.Id) return Forbid();

            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id", anunturi.UserId);
            return View(anunturi);
        }

        // POST: Anunturi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Anunturi anunturiForm, IEnumerable<IFormFile>? imaginiUpload)
        {
            if (id != anunturiForm.ID_Anunt) return NotFound();

            var anuntDinDb = await _context.Anunt
                                       .Include(a => a.GalerieImagini)
                                       .FirstOrDefaultAsync(a => a.ID_Anunt == id);

            if (anuntDinDb == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || anuntDinDb.UserId != user.Id) return Forbid();

            anuntDinDb.Marca = anunturiForm.Marca;
            anuntDinDb.Model = anunturiForm.Model;
            anuntDinDb.Pret = anunturiForm.Pret;
            anuntDinDb.An_Fabricatie = anunturiForm.An_Fabricatie;
            anuntDinDb.Kilometraj = anunturiForm.Kilometraj;
            anuntDinDb.Locatie = anunturiForm.Locatie;
            anuntDinDb.Descriere = anunturiForm.Descriere;

            if (imaginiUpload != null && imaginiUpload.Any())
            {
                foreach (var img in imaginiUpload)
                {
                    if (img.Length > 0)
                    {
                        using (var stream = new MemoryStream())
                        {
                            await img.CopyToAsync(stream);
                            var imagineNoua = new ImaginiAnunt
                            {
                                ID_Anunt = anuntDinDb.ID_Anunt,
                                Imagine = stream.ToArray()
                            };
                            _context.ImaginiAnunt.Add(imagineNoua);
                        }
                    }
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Eroare la salvare: " + ex.Message);
            }

            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id", anuntDinDb.UserId);
            return View(anuntDinDb);
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
            if (anunturi == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null || anunturi.UserId != user.Id) return Forbid();

            _context.Anunt.Remove(anunturi);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool AnunturiExists(int id)
        {
            return _context.Anunt.Any(e => e.ID_Anunt == id);
        }

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

            return Ok();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StergeImagine(int id)
        {
            var imagine = await _context.ImaginiAnunt.FindAsync(id);
            if (imagine == null) return NotFound();

            var anunt = await _context.Anunt.FindAsync(imagine.ID_Anunt);
            var user = await _userManager.GetUserAsync(User);

            if (anunt == null || user == null || anunt.UserId != user.Id)
            {
                return Forbid();
            }

            _context.ImaginiAnunt.Remove(imagine);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Edit), new { id = anunt.ID_Anunt });
        }
    }
}