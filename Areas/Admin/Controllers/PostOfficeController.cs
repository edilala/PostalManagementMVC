using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Extensions;
using PostalManagementMVC.Utilities;
using System.Diagnostics.Metrics;

namespace PostalManagementMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PostOfficeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostOfficeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PostOffice - shfaq listen e zyrave postare nga databaza
        public async Task<IActionResult> Index(int cityId)
        {
            var offices = new List<PostOffice>();
            ViewBag.PageTitle = "All Post Offices";

            if (cityId != 0)
            {
                offices = await _context.PostOffice.Where(office => office.CityId == cityId).ToListAsync();
                ViewBag.CityId = cityId;
                var city = await _context.City.Where(c => c.Id == cityId).FirstOrDefaultAsync();
                ViewBag.PageTitle = city?.Name + " Post Offices";
            }
            else
            {
                offices = await _context.PostOffice.ToListAsync();
            }

            return View(offices);
        }

        // GET: Details - kontrollon nese kemi ne databaze zyre postare me kte id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PostOffice == null)
            {
                return NotFound();
            }

            var postOffice = await _context.PostOffice
                .FirstOrDefaultAsync(m => m.Id == id);
            if (postOffice == null)
            {
                return NotFound();
            }

            ViewBag.CityName = (await _context.City.FirstOrDefaultAsync(city => city.Id == postOffice.CityId))?.Name;

            return View(postOffice);
        }

        // GET: Create - krijimi i zyrave postare
        public async Task<IActionResult> Create(int cityId)
        {
            if (cityId == 0)
            {
                ViewBag.CitiesList = (await _context.City.ToListAsync()).ConvertToSelectList<City>(-1);
            }

            return View(new PostOffice() { CityId = cityId });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,CityId,Address,ContactDetails,ManagerId")] PostOffice postOffice)
        {
            if (ModelState.IsValid)
            {
                if (!Helper.IsValidPhoneNumber(postOffice.ContactDetails))
                {
                    ModelState.AddModelError("ContactDetails", "The phone number format is invalid");
                    return View(postOffice);
                }
                _context.Add(postOffice);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { cityId = postOffice.CityId });
            }
            return View(postOffice);
        }

        // GET: Edit - editimi i nje zyre postare egziztuese ne databaze
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PostOffice == null)
            {
                return NotFound();
            }

            var postOffice = await _context.PostOffice.FindAsync(id);
            if (postOffice == null)
            {
                return NotFound();
            }

            ViewBag.CityName = (await _context.City.FirstOrDefaultAsync(city => city.Id == postOffice.CityId))?.Name;

            return View(postOffice);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,CityId,Address,ContactDetails,ManagerId")] PostOffice postOffice)
        {
            if (id != postOffice.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(postOffice);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostOfficeExists(postOffice.Id))
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
            return View(postOffice);
        }

        // GET: Delete - forma per konfirmin e fshirjes se nje zyre postare dhe metoda qe ben fshirjen
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PostOffice == null)
            {
                return NotFound();
            }

            var postOffice = await _context.PostOffice
                .FirstOrDefaultAsync(m => m.Id == id);
            if (postOffice == null)
            {
                return NotFound();
            }


            var postOfficeLocation = await (from postOffices in _context.PostOffice
                                            where postOffices.Id == id
                                            join locations in _context.Location
                                            on postOffices.Id equals locations.PostOfficeInChargeId
                                            select new Location()
                                            {
                                                Id = locations.Id
                                            }).FirstOrDefaultAsync();

            if (postOfficeLocation != null)
            {
                ModelState.AddModelError("", "You cannot delete this Post Office. Existing locations are present for this Postal Office.");
            }
            ViewBag.HasActiveLocations = postOfficeLocation != null;

            ViewBag.CityName = (await _context.City.FirstOrDefaultAsync(city => city.Id == postOffice.CityId))?.Name;

            return View(postOffice);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PostOffice == null)
            {
                return Problem("Entity set 'ApplicationDbContext.PostOffice'  is null.");
            }
            var postOffice = await _context.PostOffice.FindAsync(id);
            if (postOffice != null)
            {
                _context.PostOffice.Remove(postOffice);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostOfficeExists(int id)
        {
            return (_context.PostOffice?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
