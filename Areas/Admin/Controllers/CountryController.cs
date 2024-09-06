using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;

namespace PostalManagementMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CountryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CountryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Country - shfaq  listen e shteteve nga databaza
        public async Task<IActionResult> Index()
        {
              return _context.Country != null ? 
                          View(await _context.Country.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Country'  is null.");
        }

        // GET: Details - kontrollon nese kemi ne databaze shtet me kte id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Country == null)
            {
                return NotFound();
            }

            var country = await _context.Country
                .FirstOrDefaultAsync(m => m.Id == id);
            if (country == null)
            {
                return NotFound();
            }

            return View(country);
        }

        // GET: Create - krijimi i shteteve
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Country country)
        {
            if (ModelState.IsValid)
            {
                _context.Add(country);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(country);
        }

        // GET: Edit - editimi i nje shteti egziztues ne databaze
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Country == null)
            {
                return NotFound();
            }

            var country = await _context.Country.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }
            return View(country);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Country country)
        {
            if (id != country.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(country);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CountryExists(country.Id))
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
            return View(country);
        }

        // GET: Delete - forma per konfirmin e fshirjes se nje shteti dhe metoda qe ben fshirjen
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Country == null)
            {
                return NotFound();
            }

            var country = await _context.Country
                .FirstOrDefaultAsync(m => m.Id == id);
            if (country == null)
            {
                return NotFound();
            }

            var countryPostOfficeLocation = await (from countries in _context.Country
                                                   where countries.Id == country.Id
                                                   join cities in _context.City
                                                   on countries.Id equals cities.CountryId
                                                   join postOffices in _context.PostOffice
                                                   on cities.Id equals postOffices.CityId
                                                   join locations in _context.Location
                                                   on postOffices.Id equals locations.PostOfficeInChargeId
                                                   select new Location()
                                                   {
                                                       Id = locations.Id
                                                   }).FirstOrDefaultAsync();

            if(countryPostOfficeLocation != null) {
                ModelState.AddModelError("", "You cannot delete this country. Existing locations are present for postal offices of this country.");
            }
            ViewBag.HasActiveLocations = countryPostOfficeLocation != null;

            return View(country);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Country == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Country'  is null.");
            }
            var country = await _context.Country.FindAsync(id);
            if (country != null)
            {
                _context.Country.Remove(country);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CountryExists(int id)
        {
          return (_context.Country?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
