using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;

namespace Platforma_pentru_tranzactii_auto.Controllers
{
    [Authorize] // Doar utilizatorii logați pot folosi mesageria
    public class MesajeController : Controller
    {
        private readonly PlatformaDbContext _context;
        private readonly UserManager<Utilizator> _userManager;

        public MesajeController(PlatformaDbContext context, UserManager<Utilizator> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. INBOX: Afișează mesajele primite de utilizatorul curent
        // 1. INBOX: Afișează TOATE mesajele (primite + trimise)
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            var mesaje = await _context.Mesaje
                .Include(m => m.Expeditor)  // Ca să vedem cine a trimis
                .Include(m => m.Destinatar) // 🔥 NOU: Ca să vedem cui i-am trimis noi
                .Include(m => m.Anunt)      // Ca să vedem despre ce mașină e vorba
                                            // 🔥 MODIFICARE AICI: Luăm mesajele unde suntem destinatar SAU expeditor
                .Where(m => m.DestinatarId == user.Id || m.ExpeditorId == user.Id)
                .OrderByDescending(m => m.DataTrimiterii)
                .ToListAsync();

            return View(mesaje);
        }

        // 2. TRIMITE: Acțiunea apelată din Modal-ul de pe pagina de Detalii
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Trimite(int anuntId, int destinatarId, string continut)
        {
            var expeditor = await _userManager.GetUserAsync(User);

            if (expeditor == null) return RedirectToAction("Login", "Account");

            // Validare: Nu îți poți trimite mesaje singur
            if (expeditor.Id == destinatarId)
            {
                // Întoarcem utilizatorul la anunț cu un mesaj de eroare (opțional prin TempData)
                return RedirectToAction("Details", "Anunturi", new { id = anuntId });
            }

            if (!string.IsNullOrWhiteSpace(continut))
            {
                var mesajNou = new Mesaje
                {
                    ExpeditorId = expeditor.Id,
                    DestinatarId = destinatarId,
                    ID_Anunt = anuntId,
                    Continut = continut,
                    DataTrimiterii = DateTime.UtcNow
                };

                _context.Mesaje.Add(mesajNou);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Mesajul a fost trimis cu succes!";
            }

            // Ne întoarcem la pagina anunțului
            return RedirectToAction("Details", "Anunturi", new { id = anuntId });
        }

        // 3. RASPUNDE: Acțiunea apelată din Inbox (Modal)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Raspunde(int anuntId, int destinatarId, string continut)
        {
            var expeditor = await _userManager.GetUserAsync(User);
            if (expeditor == null) return RedirectToAction("Login", "Account");

            if (!string.IsNullOrWhiteSpace(continut))
            {
                var mesajNou = new Mesaje
                {
                    ExpeditorId = expeditor.Id,
                    DestinatarId = destinatarId,
                    ID_Anunt = anuntId,
                    Continut = continut,
                    DataTrimiterii = DateTime.UtcNow
                };

                _context.Mesaje.Add(mesajNou);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Răspunsul a fost trimis!";
            }

            // Rămânem în Inbox
            return RedirectToAction(nameof(Index));
        }

        // 4. STERGE: Șterge un mesaj din Inbox
        // POST: Șterge Mesaj
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sterge(int id)
        {
            var mesaj = await _context.Mesaje.FindAsync(id);

            if (mesaj == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);

            // 🔥 MODIFICAREA E AICI 🔥
            // Verificăm dacă userul este implicat în conversație (fie ca expeditor, fie ca destinatar)
            if (user == null || (mesaj.DestinatarId != user.Id && mesaj.ExpeditorId != user.Id))
            {
                return Forbid(); // Dacă nu ești niciunul, nu ai voie să ștergi
            }

            _context.Mesaje.Remove(mesaj);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Mesajul a fost șters.";
            return RedirectToAction(nameof(Index));
        }
    }
}