using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Platforma_pentru_tranzactii_auto.Models;

namespace Platforma_pentru_tranzactii_auto.Controllers
{
    public class ContactController : Controller
    {
        private readonly PlatformaDbContext _context;

        public ContactController(PlatformaDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var mesaje = await _context.Contact
                                       .OrderByDescending(m => m.DataTrimiterii)
                                       .ToListAsync();
            return View(mesaje);
        }

 
        public IActionResult Create()
        {
            return View();
        }

        // Create (POST): Proceseaza trimiterea
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contact model)
        {
            if (ModelState.IsValid)
            {
                var mesajNou = new Contact
                {
                    Nume = model.Nume,
                    Email = model.Email,
                    Subiect = model.Subiect,
                    Mesaj = model.Mesaj,
                    DataTrimiterii = DateTime.UtcNow,
                    EsteCitit = false
                };

                _context.Contact.Add(mesajNou);
                await _context.SaveChangesAsync();

                ViewBag.Message = "Mesajul a fost trimis cu succes!";
                ModelState.Clear();
                return View();
            }
            return View(model);
        }


        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var mesaj = await _context.Contact.FindAsync(id);
            if (mesaj == null) return NotFound();

            return View(mesaj);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Contact mesajFormular)
        {
            if (id != mesajFormular.Id) return NotFound();

            var mesajDinDb = await _context.Contact.FindAsync(id);

            if (mesajDinDb == null) return NotFound();

            if (ModelState.IsValid)
            {
                mesajDinDb.Nume = mesajFormular.Nume;
                mesajDinDb.Email = mesajFormular.Email;
                mesajDinDb.Subiect = mesajFormular.Subiect;
                mesajDinDb.Mesaj = mesajFormular.Mesaj;
                mesajDinDb.EsteCitit = mesajFormular.EsteCitit;


                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mesajFormular);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var mesaj = await _context.Contact.FindAsync(id);
            if (mesaj != null)
            {
                _context.Contact.Remove(mesaj);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}