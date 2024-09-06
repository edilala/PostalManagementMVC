using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;

namespace PostalManagementMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MailCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MailCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MailCategory - shfaq listen e kategorive te postave nga databaza
        public async Task<IActionResult> Index()
        {
              return _context.MailCategory != null ? 
                          View(await _context.MailCategory.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.MailCategory'  is null.");
        }

        // GET: Details - kontrollon nese kemi ne databaze kategori te postave me kte id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.MailCategory == null)
            {
                return NotFound();
            }

            var mailCategory = await _context.MailCategory
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mailCategory == null)
            {
                return NotFound();
            }

            return View(mailCategory);
        }

        // GET: Create - krijimi i kategorive te postave
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Fee")] MailCategory mailCategory)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mailCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mailCategory);
        }

        // GET: Edit - editimi i nje kategorie poste egziztuese ne databaze
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.MailCategory == null)
            {
                return NotFound();
            }

            var mailCategory = await _context.MailCategory.FindAsync(id);
            if (mailCategory == null)
            {
                return NotFound();
            }
            return View(mailCategory);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Fee")] MailCategory mailCategory)
        {
            if (id != mailCategory.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mailCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MailCategoryExists(mailCategory.Id))
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
            return View(mailCategory);
        }

        // Delete - forma per konfirmin e fshirjes se nje shteti dhe metoda qe ben fshirjen
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.MailCategory == null)
            {
                return NotFound();
            }

            var mailCategory = await _context.MailCategory
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mailCategory == null)
            {
                return NotFound();
            }

            var mailForLocation = await _context.Mail.FirstOrDefaultAsync(m => m.CategoryId == id);

            if (mailForLocation != null)
            {
                ModelState.AddModelError("", "You cannot delete this Category. Existing mails for this category.");
            }

            ViewBag.HasActiveMails = mailForLocation != null;


            return View(mailCategory);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.MailCategory == null)
            {
                return Problem("Entity set 'ApplicationDbContext.MailCategory'  is null.");
            }
            var mailCategory = await _context.MailCategory.FindAsync(id);
            if (mailCategory != null)
            {
                _context.MailCategory.Remove(mailCategory);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MailCategoryExists(int id)
        {
          return (_context.MailCategory?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
