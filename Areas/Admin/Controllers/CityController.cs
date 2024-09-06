using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using System.Data;
using System.Diagnostics.Metrics;

namespace PostalManagementMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class CityController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CityController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Shfaqim gjithe qytet ose nje qytet specifik nese jepet countryId
        public async Task<IActionResult> Index(int countryId)
        {
            var cities = new List<City>();
            ViewBag.PageTitle = "All Cities";
            if (countryId != 0)
            {
                cities = await _context.City.Where(city => city.CountryId == countryId).ToListAsync();
                ViewBag.CountryId = countryId;
                var country = await _context.Country.Where(c => c.Id == countryId).FirstOrDefaultAsync();
                ViewBag.PageTitle = country?.Name + "'s Cities";
            }
            else
            {
                cities = await _context.City.ToListAsync();
            }

            return View(cities);
        }

        // GET: Details - kontrollon nese kemi ne databaze qytet me kte id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.City == null)
            {
                return NotFound();
            }

            var city = await _context.City
                .FirstOrDefaultAsync(m => m.Id == id);
            if (city == null)
            {
                return NotFound();
            }

            return View(city);
        }

        // GET: Create - krijimi i qyteteve
        public IActionResult Create(int countryId)
        {
            return View(new City() { CountryId = countryId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Longitude,Latitude,CountryId")] City city)
        {
            if (ModelState.IsValid)
            {
                if (city.Latitude > 90 || city.Latitude < -90)
                    ModelState.AddModelError("Latitude", "Out of expected range [-90, 90]");

                if (city.Longitude > 180 || city.Longitude < -180)
                    ModelState.AddModelError("Longitude", "Out of expected range [-180, 180]");

                if (ModelState.ErrorCount > 0)
                    return View(city);

                _context.Add(city);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { countryId = city.CountryId });
            }
            return View(city);
        }

        // GET: Edit - editimi i nje qyteti egziztues ne databaze
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.City == null)
            {
                return NotFound();
            }

            var city = await _context.City.FindAsync(id);
            if (city == null)
            {
                return NotFound();
            }
            return View(city);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Longitude,Latitude,CountryId")] City city)
        {
            if (id != city.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(city);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CityExists(city.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { countryId = city.CountryId });
            }
            return View(city);
        }

        // GET: Delete - forma per konfirmin e fshirjes dhe parandalimi nese kemi ndonje location te lidhur me kte qytet
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.City == null)
            {
                return NotFound();
            }

            var city = await _context.City
                .FirstOrDefaultAsync(m => m.Id == id);
            if (city == null)
            {
                return NotFound();
            }


            var cityPostOfficeLocation = await (from cities in _context.City
                                                where cities.Id == id
                                                join postOffices in _context.PostOffice
                                                on cities.Id equals postOffices.CityId
                                                join locations in _context.Location
                                                on postOffices.Id equals locations.PostOfficeInChargeId
                                                select new Location()
                                                {
                                                    Id = locations.Id
                                                }).FirstOrDefaultAsync();

            if (cityPostOfficeLocation != null)
            {
                ModelState.AddModelError("", "You cannot delete this city. Existing locations are present for postal offices of this city.");
            }
            ViewBag.HasActiveLocations = cityPostOfficeLocation != null;

            return View(city);
        }

        // POST: Delete - fshirja e qytetit
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.City == null)
            {
                return Problem("Entity set 'ApplicationDbContext.City'  is null.");
            }
            var city = await _context.City.FindAsync(id);
            if (city != null)
            {
                _context.City.Remove(city);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { countryId = city.CountryId });
        }

        private bool CityExists(int id)
        {
            return (_context.City?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
