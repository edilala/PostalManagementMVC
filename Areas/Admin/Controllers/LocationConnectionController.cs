using Humanizer.Localisation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Extensions;
using PostalManagementMVC.Utilities;
using QuikGraph;
using QuikGraph.Algorithms;
using QuikGraph.Collections;

namespace PostalManagementMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class LocationConnectionController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Helper _helper;

        public LocationConnectionController(ApplicationDbContext context)
        {
            _context = context;
            _helper = new Helper(context);
        }

        // GET: LocationConnection - shfaq vendodhjet e lidhura ose ato qe nisin nga fromLocationId
        public async Task<IActionResult> Index(int fromLocationId)
        {

            var locationConns = new List<LocationConnection>();
            var locationsConnsQuery = (from conns in _context.LocationConnection
                                       join startLocations in _context.Location
                                       on conns.FromLocationId equals startLocations.Id
                                       join endLocations in _context.Location
                                       on conns.ToLocationId equals endLocations.Id
                                       select new LocationConnection()
                                       {
                                           Id = conns.Id,
                                           FromLocationId = conns.FromLocationId,
                                           ToLocationId = conns.ToLocationId,
                                           Cost = conns.Cost,
                                           OnDay = conns.OnDay,
                                           TransportDays = conns.TransportDays,
                                           FromLocationName = startLocations.Name,
                                           ToLocationName = endLocations.Name,
                                       }
                                   );
            if (fromLocationId != 0)
            {
                locationConns = await locationsConnsQuery.Where(office => office.FromLocationId == fromLocationId).ToListAsync();
                ViewBag.FromLocationId = fromLocationId;
            }
            else
            {
                locationConns = await locationsConnsQuery.ToListAsync();
            }

            return View(locationConns);
        }

        // GET: Details - kontrollon nese kemi ne databaze lidhje vendodhjesh me kte id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.LocationConnection == null)
            {
                return NotFound();
            }

            LocationConnection? locationConnection = await _helper.GetLocationConn(id);

            if (locationConnection == null)
            {
                return NotFound();
            }
            ViewBag.WeekDays = Helper.GetWeekDaysDropdownElements();

            return View(locationConnection);
        }

        // GET: Create - krijimi i nje lidhje vendodhjesh
        public async Task<IActionResult> Create(int fromLocationId)
        {
            LocationConnection conn = new LocationConnection();
            if (fromLocationId != 0)
            {
                conn.FromLocationId = fromLocationId;
            }

            ViewBag.WeekDays = Helper.GetWeekDaysDropdownElements();
            ViewBag.LocationsList = (await _context.Location.Where(loc => loc.Id != fromLocationId).ToListAsync()).ConvertToSelectList<Location>(-1);

            return View(conn);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FromLocationId,ToLocationId,OnDay,Cost,TransportDays")] LocationConnection locationConnection)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (ModelState.ErrorCount == 0)
                    {
                        _context.Add(locationConnection);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
                catch (Exception ex)
                {
                    string customErrorMess = Helper.LocationConnCreationCustomErrorMessageHandler(ex);
                    ModelState.AddModelError("", customErrorMess);
                    locationConnection.FromLocationId = 0;
                }
            }

            ViewBag.LocationsList = (await _context.Location.ToListAsync()).ConvertToSelectList<Location>(-1);
            ViewBag.WeekDays = Helper.GetWeekDaysDropdownElements();
            return View(locationConnection);
        }

        // GET: Edit - editimi i nje lidhje vendodhjesh egziztuese ne databaze
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.LocationConnection == null)
            {
                return NotFound();
            }

            LocationConnection? locationConnection = await _helper.GetLocationConn(id);

            if (locationConnection == null)
            {
                return NotFound();
            }
            ViewBag.WeekDays = Helper.GetWeekDaysDropdownElements();

            return View(locationConnection);
        }



        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FromLocationId,ToLocationId,OnDay,Cost,TransportDays")] LocationConnection locationConnection)
        {
            if (id != locationConnection.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(locationConnection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LocationConnectionExists(locationConnection.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch(Exception ex)
                {
                    string customErrorMess = Helper.LocationConnCreationCustomErrorMessageHandler(ex);
                    ModelState.AddModelError("", customErrorMess);
                    return View(locationConnection);
                }
                return RedirectToAction(nameof(Index));
            }

            return View(locationConnection);
        }

        // GET: Delete - forma per konfirmin e fshirjes se nje lidhje vendodhjesh dhe metoda qe ben fshirjen
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.LocationConnection == null)
            {
                return NotFound();
            }

            LocationConnection? locationConnection = await _helper.GetLocationConn(id);

            if (locationConnection == null)
            {
                return NotFound();
            }

            return View(locationConnection);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.LocationConnection == null)
            {
                return Problem("Entity set 'ApplicationDbContext.LocationConnection'  is null.");
            }
            var locationConnection = await _context.LocationConnection.FindAsync(id);
            if (locationConnection != null)
            {
                _context.LocationConnection.Remove(locationConnection);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LocationConnectionExists(int id)
        {
            return (_context.LocationConnection?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
