using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Areas.Distributor.Models;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Interfaces;
using PostalManagementMVC.Models;
using PostalManagementMVC.Utilities;
using System.Security.Claims;

namespace PostalManagementMVC.Areas.Distributor.Controllers
{
    [Area("Distributor")]
    [Authorize(Roles = "Admin,Distributor")]
    public class MailController : AbstractController
    {

        public MailController(ApplicationDbContext context, IEmailSender emailSender) : base(context, emailSender) { }

        // GET: Distributor/Mail
        public async Task<IActionResult> Index()
        {
            var statuses = await _context.StatusCatalog.ToListAsync();

            int deliveredStateId = statuses.FirstOrDefault(s => String.Equals(s.Name, Globals.DELIVERED_TO_DESTINATION_CENTER)).Id;
            ViewBag.StatusesList = statuses;
            int outForDeliveryStatusId = statuses.FirstOrDefault(s => String.Equals(s.Name, Globals.OUT_FOR_DELIVERY)).Id;


            // to be reviewed later
            var bundleCategoryId = (await _context.MailCategory.FirstOrDefaultAsync(category => String.Equals(category.Name, Globals.BUNDLE_CATEGORY))).Id;

            // add user admin query. do not add location filter for admin
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<Mail>? mailsList = new List<Mail>();
            if (User.IsInRole("Admin"))
            {
                mailsList = await (from mails in _context.Mail
                                   where mails.CategoryId != bundleCategoryId
                                   join categories in _context.MailCategory
                                   on mails.CategoryId equals categories.Id
                                   select new Mail()
                                   {
                                       Id = mails.Id,
                                       MailBundleId = mails.MailBundleId,
                                       Code = mails.Code,
                                       RecipientAddress = mails.RecipientAddress,
                                       SenderAddress = mails.SenderAddress,
                                       StartLocationId = mails.StartLocationId,
                                       EndLocationId = mails.EndLocationId,
                                       TimeInserted = mails.TimeInserted,
                                       ReceiverContactNr = mails.ReceiverContactNr,
                                       PostalCodeId = mails.PostalCodeId,
                                       CategoryId = mails.CategoryId,
                                       CurrentStatus = (from statuses in _context.MailStatus
                                                        where statuses.MailId == mails.Id
                                                        orderby statuses.TimeAssigned descending
                                                        select new MailStatus()
                                                        {
                                                            Id = statuses.Id,
                                                            MailId = mails.Id,
                                                            StatusCatalogId = statuses.StatusCatalogId,
                                                            TimeAssigned = statuses.TimeAssigned
                                                        }).FirstOrDefault(),
                                       Category = new MailCategory()
                                       {
                                           Id = categories.Id,
                                           Name = categories.Name
                                       }
                                   })
                                      .Where(m => m.CurrentStatus.StatusCatalogId == deliveredStateId || m.CurrentStatus.StatusCatalogId == outForDeliveryStatusId).ToListAsync();
            }
            else
            {
                mailsList = await (from mails in _context.Mail
                                   where mails.CategoryId != bundleCategoryId
                                   join categories in _context.MailCategory
                                   on mails.CategoryId equals categories.Id
                                   join users in _context.Users
                                   on mails.EndLocationId equals users.LocationAssignedId
                                   where users.Id == userId
                                   select new Mail()
                                   {
                                       Id = mails.Id,
                                       MailBundleId = mails.MailBundleId,
                                       Code = mails.Code,
                                       RecipientAddress = mails.RecipientAddress,
                                       SenderAddress = mails.SenderAddress,
                                       StartLocationId = mails.StartLocationId,
                                       EndLocationId = mails.EndLocationId,
                                       TimeInserted = mails.TimeInserted,
                                       ReceiverContactNr = mails.ReceiverContactNr,
                                       PostalCodeId = mails.PostalCodeId,
                                       CategoryId = mails.CategoryId,
                                       CurrentStatus = (from statuses in _context.MailStatus
                                                        where statuses.MailId == mails.Id
                                                        orderby statuses.TimeAssigned descending
                                                        select new MailStatus()
                                                        {
                                                            Id = statuses.Id,
                                                            MailId = mails.Id,
                                                            StatusCatalogId = statuses.StatusCatalogId,
                                                            TimeAssigned = statuses.TimeAssigned
                                                        }).FirstOrDefault(),
                                       Category = new MailCategory()
                                       {
                                           Id = categories.Id,
                                           Name = categories.Name
                                       }
                                   })
                                      .Where(m => m.CurrentStatus.StatusCatalogId == deliveredStateId || m.CurrentStatus.StatusCatalogId == outForDeliveryStatusId).ToListAsync();
            }

            ViewBag.OutForDeliveryStatusId = outForDeliveryStatusId;
            ViewBag.DeliveredToCenterStatusId = deliveredStateId;

            return _context.Mail != null ?
                          View(mailsList) :
                          Problem("Entity set 'ApplicationDbContext.Mail'  is null.");
        }

        // GET: Distributor/Mail/Details/5
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

            var mailCategory = await _context.MailCategory.FirstOrDefaultAsync(m => m.Id == mail.CategoryId);
            ViewBag.CategoryName = mailCategory?.Name;
            List<MailStatus> list = await _helper.GetMailHistory(mail);
            ViewBag.StatusesList = new StatusHistoryModel() { Statuses = list };
            ViewBag.CurrentStatus = list.OrderByDescending(item => item.TimeAssigned).FirstOrDefault();
            ViewBag.OutForDeliveryStatusId = (await _context.StatusCatalog.FirstOrDefaultAsync(s => String.Equals(s.Name, Globals.OUT_FOR_DELIVERY))).Id;

            return View(mail);
        }

        // GET: Carrier/Mail/PickUp/5
        public async Task<IActionResult> OutForDelivery(int? id)
        {
            if (id == null || _context.Mail == null)
            {
                return NotFound();
            }

            var mail = await _helper.GetMailWithCurrentStatus(id.Value);

            if (mail == null)
            {
                return NotFound();
            }

            return View(mail);
        }

        [HttpPost, ActionName("OutForDelivery")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OutForDeliveryConfirmed(int id)
        {
            if (_context.Mail == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Mail'  is null.");
            }
            //var mail = await _context.Mail.FirstOrDefaultAsync(mailid);
            var mail = await _helper.GetMailWithCurrentStatus(id);
            //.ToListAsync();
            //var mail = mails1.FirstOrDefault();
            if (mail != null)
            {
                var registeredStatusRec = await _context.StatusCatalog.FirstOrDefaultAsync(st => String.Equals(st.Name, Globals.DELIVERED_TO_DESTINATION_CENTER));

                // only mails in delivered to center status can be picked up
                if (registeredStatusRec.Id != mail.CurrentStatus.StatusCatalogId)
                {
                    return RedirectToAction(nameof(Index));
                }
                var outForDeliveryStatus = await _context.StatusCatalog.FirstOrDefaultAsync(st => String.Equals(st.Name, Globals.OUT_FOR_DELIVERY));
                if (outForDeliveryStatus != null)
                {
                    MailStatus newStatus = new MailStatus()
                    {
                        MailId = id,
                        StatusCatalogId = outForDeliveryStatus.Id,
                        TimeAssigned = DateTime.UtcNow,
                        OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    };


                    _context.Add(newStatus);
                }

                await _context.SaveChangesAsync();
                // set mail as modified by distributor

                try
                {
                    // retrieve again from db
                    // tracking problem with using the object above
                    var mailToUpdate = await _context.Mail.FindAsync(id);
                    mailToUpdate.ModifiedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _context.Mail.Update(mailToUpdate);
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

                await NotifyUserMailStatusChanged(id, Globals.OUT_FOR_DELIVERY);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Carrier/Mail/Delivered/5
        public async Task<IActionResult> Delivered(int? id)
        {
            if (id == null || _context.Mail == null)
            {
                return NotFound();
            }

            var mail = await _helper.GetMailWithCurrentStatus(id.Value);
            if (mail == null)
            {
                return NotFound();
            }

            var outForDeliveryStatus = await _context.StatusCatalog.FirstOrDefaultAsync(st => String.Equals(st.Name, Globals.OUT_FOR_DELIVERY));
            if (mail.CurrentStatus.StatusCatalogId != outForDeliveryStatus.Id)
            {
                return BadRequest("This item was not for delivery");
            }

            return View(mail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delivered([Bind("Id")] Mail mail)
        {
            if (mail.Id != 0)
            {
                try
                {
                    Mail mailToUpdate = await _context.Mail.FirstOrDefaultAsync(m => m.Id == mail.Id);
                    if (mailToUpdate != null)
                    {
                        // create new status to delivered for mail
                        var mailStatusToDelivered = new MailStatus()
                        {
                            MailId = mailToUpdate.Id,
                            OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            TimeAssigned = DateTime.UtcNow,
                            StatusCatalogId = (await _context.StatusCatalog.FirstOrDefaultAsync(s => String.Equals(s.Name, Globals.DELIVERED))).Id
                        };

                        _context.Add(mailStatusToDelivered);
                        mailToUpdate.TimeDelivered = DateTime.UtcNow;
                        mailToUpdate.ModifiedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

                        await _context.SaveChangesAsync();
                        await NotifyUserMailStatusChanged(mailToUpdate.Id, Globals.DELIVERED);

                        var subscription = await _context.ClientSubscription.FirstOrDefaultAsync(s => s.TrackedMailId == mailToUpdate.Id);
                        if (subscription != null)
                        {
                            _context.ClientSubscription.Remove(subscription);
                            await _context.SaveChangesAsync();
                        }

                    }
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
            return NotFound();
        }


        // GET: Carrier/Mail/Returned/5
        public async Task<IActionResult> Returned(int? id)
        {
            if (id == null || _context.Mail == null)
            {
                return NotFound();
            }

            var mail = await _helper.GetMailWithCurrentStatus(id.Value);
            if (mail == null)
            {
                return NotFound();
            }

            return View(mail);
        }

        [HttpPost, ActionName("Returned")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnedConfirmed([Bind("MailId,Note")] MailStatus mailStatus)
        {
            if (mailStatus != null && MailExists(mailStatus.MailId))
            {
                try
                {
                    var existingMail = await _context.Mail.FirstOrDefaultAsync(m => m.Id == mailStatus.MailId);
                    if (existingMail == null)
                        return NotFound();
                    // create new status for mail
                    var packageStatusToDelivered = new MailStatus()
                    {
                        MailId = mailStatus.MailId,
                        OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        TimeAssigned = DateTime.UtcNow,
                        Note = mailStatus.Note,
                        StatusCatalogId = (await _context.StatusCatalog.FirstOrDefaultAsync(s => String.Equals(s.Name, Globals.RETURNED))).Id
                    };
                    // after setting the returned status for the mail
                    // set its status to registered
                    // so the carriers with continue to handle the item to the new destination
                    var packageStatusToReRegistered = new MailStatus()
                    {
                        MailId = mailStatus.MailId,
                        OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                        TimeAssigned = DateTime.UtcNow.AddMinutes(1),
                        StatusCatalogId = (await _context.StatusCatalog.FirstOrDefaultAsync(s => String.Equals(s.Name, Globals.REGISTERED))).Id
                    };
                    _context.Add(packageStatusToDelivered);
                    _context.Add(packageStatusToReRegistered);

                    // swap start with end locations
                    var previousStartLoc = existingMail.StartLocationId;
                    existingMail.StartLocationId = existingMail.EndLocationId;
                    existingMail.EndLocationId = previousStartLoc;

                    var previousSenderAddr = existingMail.SenderAddress;
                    existingMail.SenderAddress = existingMail.RecipientAddress;
                    existingMail.RecipientAddress = previousSenderAddr;

                    existingMail.ModifiedById = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    await _context.SaveChangesAsync();

                    await NotifyUserMailStatusChanged(mailStatus.MailId, Globals.RETURNED);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MailExists(mailStatus.Id))
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
            return NotFound();
        }
        private bool MailExists(int id)
        {
            return (_context.Mail?.Any(e => e.Id == id)).GetValueOrDefault();
        }




        //
        // Reports
        //
        public async Task<IActionResult> Reports()
        {
            return View(new ReportControls()
            {
                FromDate = DateTime.Now.AddYears(-1),
                ToDate = DateTime.Now
            });
        }
        [HttpPost]
        public async Task<List<KeyValuePair<string, double>>> GetCounterChartData([FromBody] GetChartDataRequest req)
        {
            // default values
            DateTime fromDate = DateTime.Now.AddYears(-1);
            DateTime toDate = DateTime.Now;

            // get them from request if present
            if (req != null)
            {
                if (req.FromDate != DateTime.MinValue)
                    fromDate = req.FromDate;
                if (req.ToDate != DateTime.MinValue)
                    toDate = req.ToDate;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);

            if (currentUser == null)
                return new List<KeyValuePair<string, double>>();

            List<Mail>? counterMailsInTimeWindow = await (from mails in _context.Mail
                                                          where mails.TimeInserted >= fromDate && mails.TimeInserted <= toDate
                                                          join users in _context.Users
                                                          on mails.ModifiedById equals users.Id
                                                          where users.Id == userId
                                                          join statuses in _context.MailStatus
                                                          on mails.Id equals statuses.MailId
                                                          join statusesCatalog in _context.StatusCatalog
                                                          on statuses.StatusCatalogId equals statusesCatalog.Id
                                                          select new Mail()
                                                          {
                                                              Id = mails.Id,
                                                              ModifiedById = mails.ModifiedById,
                                                              CurrentStatus = new MailStatus()
                                                              {
                                                                  Id = statuses.Id,
                                                                  MailId = mails.Id,
                                                                  StatusCatalogId = statusesCatalog.Id,
                                                                  StatusName = statusesCatalog.Name,
                                                                  OwnerId = statuses.OwnerId
                                                              },
                                                          }
                                      ).ToListAsync();



            var data = counterMailsInTimeWindow.GroupBy(x => x.CurrentStatus.StatusCatalogId)
                            .Select(m => new KeyValuePair<string, double>(m.Select(ml => ml.CurrentStatus.StatusName).FirstOrDefault(), m.Count())).OrderBy(x => x.Key).ToList();

            return data;
        }
    }
}
