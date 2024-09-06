using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Extensions;
using PostalManagementMVC.Interfaces;
using PostalManagementMVC.Utilities;

namespace PostalManagementMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PostalCodeController : AbstractController
    {
        public PostalCodeController(ApplicationDbContext context, IEmailSender emailSender) : base(context, emailSender) { }

        // GET: PostalCode - shfaq  listen e kodeve postare nga databaza
        public async Task<IActionResult> Index(int fromLocationId)
        {
            var postalCodes = new List<PostalCode>();
            var postalCodesQuery = (from zipcode in _context.PostalCode
                                    join locations in _context.Location
                                    on zipcode.LocationId equals locations.Id
                                    select new PostalCode()
                                    {
                                        Id = zipcode.Id,
                                        ZipCode = zipcode.ZipCode,
                                        LocationId = locations.Id,
                                        LocationName = locations.Name
                                    }
                                   );
            if (fromLocationId != 0)
            {
                postalCodes = await postalCodesQuery.Where(postalCode => postalCode.LocationId == fromLocationId).ToListAsync();
                ViewBag.FromLocationId = fromLocationId;
            }
            else
            {
                postalCodes = await postalCodesQuery.ToListAsync();
            }

            return View(postalCodes);
        }

        // GET: Details - kontrollon nese kemi ne databaze kod postar me kte id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.PostalCode == null)
            {
                return NotFound();
            }

            var postalCode = await _helper.GetPostalCodeById(id);
            if (postalCode == null)
            {
                return NotFound();
            }

            return View(postalCode);
        }

        // GET: Create - krijimi i kodeve postare
        public async Task<IActionResult> Create(int fromLocationId)
        {
            if (fromLocationId == 0)
                ViewBag.Locations = (await _context.Location.ToListAsync()).ConvertToSelectList<Location>(fromLocationId);
            return View(new PostalCode() { LocationId = fromLocationId });
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ZipCode,LocationId")] PostalCode postalCode)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(postalCode);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch(Exception ex)
                {
                    string customErrorMess = Helper.PostalCodeCreationCustomErrorMessageHandler(ex);
                    ModelState.AddModelError("", customErrorMess);

                    ViewBag.Locations = (await _context.Location.ToListAsync()).ConvertToSelectList<Location>(-1);
                    
                }
            }
            return View(postalCode);
        }

        // GET: Edit - editimi i nje kodi postar egziztues ne databaze
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.PostalCode == null)
            {
                return NotFound();
            }

            var postalCode = await _helper.GetPostalCodeById(id);

            if (postalCode == null)
            {
                return NotFound();
            }
            
            ViewBag.Locations = (await _context.Location.ToListAsync()).ConvertToSelectList<Location>(id.Value);

            return View(postalCode);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ZipCode,LocationId")] PostalCode postalCode)
        {
            if (id != postalCode.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(postalCode);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostalCodeExists(postalCode.Id))
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
            return View(postalCode);
        }

        // GET: Delete - forma per konfirmin e fshirjes se nje kodi postar dhe metoda qe ben fshirjen
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.PostalCode == null)
            {
                return NotFound();
            }

            var postalCode = await _helper.GetPostalCodeById(id);

            if (postalCode == null)
            {
                return NotFound();
            }

            return View(postalCode);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.PostalCode == null)
            {
                return Problem("Entity set 'ApplicationDbContext.PostalCode'  is null.");
            }
            var postalCode = await _context.PostalCode.FindAsync(id);
            if (postalCode != null)
            {
                _context.PostalCode.Remove(postalCode);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostalCodeExists(int id)
        {
            return (_context.PostalCode?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
