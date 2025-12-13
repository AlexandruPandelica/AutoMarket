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
                // Includem comentariile și autorii lor
                .Include(a => a.Comentari).ThenInclude(c => c.User)
                // 🔥 INCLUDEM GALERIA DE IMAGINI 🔥
                .Include(a => a.GalerieImagini)
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
        public async Task<IActionResult> Create([Bind("ID_Anunt,Marca,Model,Pret,An_Fabricatie,Kilometraj,Descriere,Locatie,UserId")] Anunturi anunturi, IEnumerable<IFormFile>? imaginiUpload)
        {
            // 1. Curățăm validarea pentru câmpurile automate
            ModelState.Remove("User");
            ModelState.Remove("Imagine_Anunt");
            ModelState.Remove("GalerieImagini"); // Nu validăm lista, că o populăm noi

            // 2. Setări automate
            anunturi.Data_Postarii = DateTime.UtcNow;
            anunturi.Nr_Vizualizari = 0;

            // 3. Procesăm IMAGINEA DE COPERTĂ (Pentru lista de anunțuri)
            // Luăm prima imagine din listă și o punem ca "Thumbnail"
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
                // A. Salvăm Anunțul ÎNTÂI (pentru a se genera ID_Anunt)
                _context.Add(anunturi);
                await _context.SaveChangesAsync();

                // B. Acum salvăm GALERIA DE IMAGINI
                if (imaginiUpload != null && imaginiUpload.Count() > 0)
                {
                    foreach (var img in imaginiUpload)
                    {
                        using (var stream = new MemoryStream())
                        {
                            await img.CopyToAsync(stream);

                            var imagineNoua = new ImaginiAnunt
                            {
                                ID_Anunt = anunturi.ID_Anunt, // Aici folosim ID-ul generat mai sus
                                Imagine = stream.ToArray()    // ✅ Aici folosim proprietatea ta "Imagine"
                            };

                            _context.ImaginiAnunt.Add(imagineNoua);
                        }
                    }
                    // Salvăm din nou pentru a scrie imaginile în tabelul lor
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

            // 🔥 MODIFICARE: Folosim Include pentru a aduce și galeria
            var anunturi = await _context.Anunt
                .Include(a => a.GalerieImagini)
                .FirstOrDefaultAsync(m => m.ID_Anunt == id);

            if (anunturi == null) return NotFound();

            // Verificare securitate: Doar proprietarul poate edita
            var user = await _userManager.GetUserAsync(User);
            if (user == null || anunturi.UserId != user.Id) return Forbid();

            ViewData["UserId"] = new SelectList(_context.Utilizatori, "Id", "Id", anunturi.UserId);
            return View(anunturi);
        }

        // 1. Adaugam parametrul IFormFile? imagineUpload
        // POST: Anunturi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Anunturi anunturiForm, IEnumerable<IFormFile>? imaginiUpload)
        {
            if (id != anunturiForm.ID_Anunt)
            {
                return NotFound();
            }

            // 1. Căutăm anunțul REAL din baza de date
            var anuntDinDb = await _context.Anunt
                                           .Include(a => a.GalerieImagini)
                                           .FirstOrDefaultAsync(a => a.ID_Anunt == id);

            if (anuntDinDb == null)
            {
                return NotFound();
            }

            // 2. Verificăm permisiunile
            var user = await _userManager.GetUserAsync(User);
            if (user == null || anuntDinDb.UserId != user.Id)
            {
                return Forbid();
            }

            // 3. Actualizăm MANUAL datele
            // Asta garantează că nu ne blocăm în validări inutile
            anuntDinDb.Marca = anunturiForm.Marca;
            anuntDinDb.Model = anunturiForm.Model;
            anuntDinDb.Pret = anunturiForm.Pret;
            anuntDinDb.An_Fabricatie = anunturiForm.An_Fabricatie;
            anuntDinDb.Kilometraj = anunturiForm.Kilometraj;
            anuntDinDb.Locatie = anunturiForm.Locatie;
            anuntDinDb.Descriere = anunturiForm.Descriere;

            // 4. Gestionăm imaginile noi (dacă există)
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

            // 5. Salvăm și redirecționăm
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index)); // <--- Aici te trimite înapoi la listă
            }
            catch (Exception ex)
            {
                // Dacă apare o eroare tehnică la scriere, o afișăm în pagină
                ModelState.AddModelError("", "Eroare la salvare: " + ex.Message);
            }

            // Reîncărcare date necesare pentru View în caz de eroare
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


        // POST: Șterge o singură imagine din galerie
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StergeImagine(int id)
        {
            var imagine = await _context.ImaginiAnunt.FindAsync(id);
            if (imagine == null) return NotFound();

            // Verificare securitate (găsim anunțul părintesc)
            var anunt = await _context.Anunt.FindAsync(imagine.ID_Anunt);
            var user = await _userManager.GetUserAsync(User);

            if (anunt == null || user == null || anunt.UserId != user.Id)
            {
                return Forbid();
            }

            // Ștergem imaginea
            _context.ImaginiAnunt.Remove(imagine);
            await _context.SaveChangesAsync();

            // Ne întoarcem la pagina de Editare a acelui anunț
            return RedirectToAction(nameof(Edit), new { id = anunt.ID_Anunt });
        }

    }
}