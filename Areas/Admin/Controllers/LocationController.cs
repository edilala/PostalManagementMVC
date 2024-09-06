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
    public class LocationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Location - shfaq  listen e vendodhjeve nga databaza
        public async Task<IActionResult> Index(int officeId)
        {
            var locations = new List<Location>();
            ViewBag.PageTitle = "All Locations";

            if (officeId != 0)
            {
                locations = await _context.Location.Where(office => office.PostOfficeInChargeId == officeId).ToListAsync();
                ViewBag.OfficeIdInCharge = officeId;
                var postalOffice = await _context.PostOffice.FirstOrDefaultAsync(p => p.Id == officeId);
                ViewBag.PageTitle = postalOffice?.Name + "'s Locations";
            }
            else
            {
                locations = await _context.Location.ToListAsync();
            }

            return View(locations);
        }

        // GET: Details - kontrollon nese kemi ne databaze vendodhje me kte id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Location == null)
            {
                return NotFound();
            }

            var location = await _context.Location
                .FirstOrDefaultAsync(m => m.Id == id);
            if (location == null)
            {
                return NotFound();
            }

            return View(location);
        }

        // GET: Create - krijimi i vendodhjeve
        public async Task<IActionResult> Create(int officeId)
        {
            if (officeId == 0)
                ViewBag.PostOfficeInChargeList = (await _context.Location.ToListAsync()).ConvertToSelectList<Location>(officeId);
            return View(new Location() { PostOfficeInChargeId = officeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,PostOfficeInChargeId,IsPostOffice,InitialPostalCodeId")] Location location)
        {
            if (ModelState.IsValid)
            {
                _context.Add(location);

                try
                {
                    await _context.SaveChangesAsync();

                    if (location.InitialPostalCodeId.HasValue && location.InitialPostalCodeId.Value != 0)
                    {
                        _context.Add(new PostalCode()
                        {
                            ZipCode = location.InitialPostalCodeId.ToString(),
                            LocationId = location.Id
                        });
                    }

                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index), new { officeId = location.PostOfficeInChargeId });
                }
                catch (Exception ex)
                {
                    string customErrorMess = Helper.PostalCodeCreationCustomErrorMessageHandler(ex);
                    if (!String.IsNullOrWhiteSpace(customErrorMess))
                        ModelState.AddModelError("", customErrorMess);
                }

            }
            return View(location);
        }

        // GET: Edit - editimi i nje vendodhje egziztuese ne databaze
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Location == null)
            {
                return NotFound();
            }

            var location = await _context.Location.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,PostOfficeInChargeId,IsPostOffice")] Location location)
        {
            if (id != location.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(location);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationExists(location.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { officeid = location.PostOfficeInChargeId });
            }
            return View(location);
        }

        // GET: Delete - forma per konfirmin e fshirjes se nje vendodhje dhe metoda qe ben fshirjen
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Location == null)
            {
                return NotFound();
            }

            var location = await _context.Location
                .FirstOrDefaultAsync(m => m.Id == id);
            if (location == null)
            {
                return NotFound();
            }


            var mailForLocation = await _context.Mail.FirstOrDefaultAsync(m => m.StartLocationId == id || m.EndLocationId == id);

            ApplicationUser? userForLocation = null;
            if(mailForLocation == null)
                userForLocation = await _context.Users.FirstOrDefaultAsync(m => m.LocationAssignedId == id);

            if (mailForLocation != null)
            {
                ModelState.AddModelError("", "You cannot delete this Location. Existing mails for this location are found.");
            }
            else if (userForLocation != null)
            {
                ModelState.AddModelError("", "You cannot delete this Location. Current users are assigned to this location.");
            }
            ViewBag.HasActiveRelatedRecords = mailForLocation != null || userForLocation != null;

            return View(location);
        }

       
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Location == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Location'  is null.");
            }
            var postOfficeId = 0;
            var location = await _context.Location.FindAsync(id);
            if (location != null)
            {
                postOfficeId = location.PostOfficeInChargeId;

                var allConnectionsOfLocation = await _context.LocationConnection.Where(c => c.FromLocationId == id || c.ToLocationId == id).ToListAsync();

                foreach(var connection in allConnectionsOfLocation)
                {
                    _context.LocationConnection.Remove(connection);
                }

                await _context.SaveChangesAsync();
                
                _context.Location.Remove(location);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), new { officeid = postOfficeId });
        }

        private bool LocationExists(int id)
        {
            return (_context.Location?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
