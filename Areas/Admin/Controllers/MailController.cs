using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Areas.Admin.Models;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Utilities;

namespace PostalManagementMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MailController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MailController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Mail - shfaq listen e postave qe as nuk jane Delivered as Canceled
        public async Task<IActionResult> Index()
        {
            var statuses = await _context.StatusCatalog.ToListAsync();
            var categoryId = (await _context.MailCategory.FirstOrDefaultAsync(category => String.Equals(category.Name, "BUNDLE"))).Id;
            int deliveredStateId = statuses.FirstOrDefault(s => String.Equals(s.Name, Globals.DELIVERED)).Id;
            int cancelledStateid = statuses.FirstOrDefault(s => String.Equals(s.Name, Globals.CANCELLED)).Id;



            var mailsList = await (from mails in _context.Mail
                                   where mails.CategoryId != categoryId && mails.TimeInserted.AddDays(mails.DaysToDelivery * 2) < DateTime.UtcNow
                                   select new Mail()
                                   {
                                       Id = mails.Id,
                                       Code = mails.Code,
                                       RecipientAddress = mails.RecipientAddress,
                                       SenderAddress = mails.SenderAddress,
                                       TimeInserted = mails.TimeInserted,
                                       TimeDelivered = mails.TimeDelivered,
                                       DaysToDelivery = mails.DaysToDelivery,
                                       OverdueDays = (DateTime.UtcNow - mails.TimeInserted.AddDays(mails.DaysToDelivery)).Days,
                                       CurrentStatus = (from statuses in _context.MailStatus
                                                        where statuses.MailId == mails.Id
                                                        orderby statuses.TimeAssigned descending
                                                        select new MailStatus()
                                                        {
                                                            Id = statuses.Id,
                                                            MailId = mails.Id,
                                                            StatusCatalogId = statuses.StatusCatalogId,
                                                            TimeAssigned = statuses.TimeAssigned
                                                        }).FirstOrDefault()
                                   }
                          ).Where(m => m.CurrentStatus.StatusCatalogId != deliveredStateId && m.CurrentStatus.StatusCatalogId != cancelledStateid).ToListAsync();

            return View(mailsList.OrderByDescending(m => m.OverdueDays));
        }

        // GET: Details - kontrollon nese kemi ne databaze ndonje poste me kte id
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Mail == null)
            {
                return NotFound();
            }

            var mail = await _context.Mail
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mail == null)
            {
                return NotFound();
            }

            return View(mail);
        }

        // GET: Create - krijimi i nje poste
        public IActionResult Create()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MailBundleId,Code,RecipientAddress,SenderAddress,StartLocationId,EndLocationId,CategoryId,PostalCodeId,TimeInserted,TimeDelivered,CreatedById,ModifiedById,Height,Width,Hight,Weight,ReceiverContactNr,ChoosenPath")] Mail mail)
        {
            if (ModelState.IsValid)
            {
                _context.Add(mail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mail);
        }

        // GET: Edit - editimi i nje poste
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Mail == null)
            {
                return NotFound();
            }

            var mail = await _context.Mail.FindAsync(id);
            if (mail == null)
            {
                return NotFound();
            }
            return View(mail);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MailBundleId,Code,RecipientAddress,SenderAddress,StartLocationId,EndLocationId,CategoryId,PostalCodeId,TimeInserted,TimeDelivered,CreatedById,ModifiedById,Height,Width,Hight,Weight,ReceiverContactNr,ChoosenPath")] Mail mail)
        {
            if (id != mail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MailExists(mail.Id))
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
            return View(mail);
        }

        // GET: Delete - forma per konfirmin e fshirjes se nje poste dhe metoda qe ben fshirjen
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Mail == null)
            {
                return NotFound();
            }

            var mail = await _context.Mail
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mail == null)
            {
                return NotFound();
            }

            return View(mail);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Mail == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Mail'  is null.");
            }
            var mail = await _context.Mail.FindAsync(id);
            if (mail != null)
            {
                _context.Mail.Remove(mail);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MailExists(int id)
        {
          return (_context.Mail?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        
        // Reports

        // Reports by city - raporton te dhena brenda nje periudhe te caktuar dhe i grupon sipas qytetit
        public async Task<IActionResult> ReportsByCity()
        {
            return View(new ReportControls()
            {
                FromDate = DateTime.Now.AddYears(-1),
                ToDate = DateTime.Now
            });
        }
        [HttpPost]
        public async Task<List<KeyValuePair<string, double>>> GetChartDataByCity([FromBody] GetChartDataRequest req)
        {
            
            DateTime fromDate = DateTime.Now.AddYears(-1);
            DateTime toDate = DateTime.Now;

            
            if (req != null)
            {
                if (req.FromDate != DateTime.MinValue)
                    fromDate = req.FromDate;
                if (req.ToDate != DateTime.MinValue)
                    toDate = req.ToDate;
            }

            List<Mail>? processedMailsInTimeWindow = await (from mails in _context.Mail
                                              where mails.TimeInserted >= fromDate && mails.TimeInserted <= toDate
                                              join locations in _context.Location
                                              on mails.StartLocationId equals locations.Id
                                              join postOffices in _context.PostOffice
                                              on locations.PostOfficeInChargeId equals postOffices.Id
                                              join cities in _context.City
                                              on postOffices.CityId equals cities.Id
                                              select new Mail()
                                              {
                                                  Id = mails.Id,
                                                  CreatedById = mails.CreatedById,
                                                  CityId = cities.Id,
                                                  CityName = cities.Name,
                                              }
                                  ).ToListAsync();

            var data = processedMailsInTimeWindow.GroupBy(x => x.CityId)
                            .Select(m => new KeyValuePair<string, double>(m.Select(ml => ml.CityName).FirstOrDefault(), m.Count())).OrderBy(x => x.Key).ToList();

            return data;
        }





        // Reports by location - raporton te dhena brenda nje periudhe te caktuar dhe i grupon sipas vendodhjes
        public async Task<IActionResult> ReportsByLocation()
        {
            return View(new ReportControls()
            {
                FromDate = DateTime.Now.AddYears(-1),
                ToDate = DateTime.Now
            });
        }

        public async Task<List<KeyValuePair<string, double>>> GetChartDataByLocation([FromBody] GetChartDataRequest req)
        {
            
            DateTime fromDate = DateTime.Now.AddYears(-1);
            DateTime toDate = DateTime.Now;

            
            if (req != null)
            {
                if (req.FromDate != DateTime.MinValue)
                    fromDate = req.FromDate;
                if (req.ToDate != DateTime.MinValue)
                    toDate = req.ToDate;
            }

            List<Mail>? processedMailsInTimeWindow = await (from mails in _context.Mail
                                                            where mails.TimeInserted >= fromDate && mails.TimeInserted <= toDate
                                                            join locations in _context.Location
                                                            on mails.StartLocationId equals locations.Id
                                                            select new Mail()
                                                            {
                                                                Id = mails.Id,
                                                                CreatedById = mails.CreatedById,
                                                                StartLocationId = locations.Id,
                                                                StartLocationName = locations.Name,
                                                            }
                                  ).ToListAsync();

            var data = processedMailsInTimeWindow.GroupBy(x => x.StartLocationId)
                            .Select(m => new KeyValuePair<string, double>(m.Select(ml => ml.StartLocationName).FirstOrDefault(), m.Count())).OrderBy(x => x.Key).ToList();

            return data;
        }
    }
}
