using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Areas.Carrier.Models;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Extensions;
using PostalManagementMVC.Interfaces;
using PostalManagementMVC.Models;
using PostalManagementMVC.Utilities;

namespace PostalManagementMVC.Areas.Carrier.Controllers
{
    [Area("Carrier")]
    [Authorize(Roles = "Admin,Carrier")]
    public class MailController : AbstractController
    {
        public MailController(ApplicationDbContext context, IEmailSender emailSender) : base(context, emailSender) { }

        // GET: Carrier/Mail
        public async Task<IActionResult> Index()
        {
            var statuses = await _context.StatusCatalog.ToListAsync();
            var categoryId = (await _context.MailCategory.FirstOrDefaultAsync(category => String.Equals(category.Name, Globals.BUNDLE_CATEGORY))).Id;
            int deliveredStateId = statuses.FirstOrDefault(s => String.Equals(s.Name, Globals.DELIVERED)).Id;

            var mailsList = await (from mails in _context.Mail
                                   where mails.CategoryId == categoryId
                                   join startLocations in _context.Location
                                   on mails.StartLocationId equals startLocations.Id
                                   join endLocations in _context.Location
                                   on mails.EndLocationId equals endLocations.Id
                                   select new Mail()
                                   {
                                       Id = mails.Id,
                                       Code = mails.Code,
                                       StartLocationName = startLocations.Name,
                                       EndLocationName = endLocations.Name,
                                       TimeInserted = mails.TimeInserted,
                                       TimeDelivered = mails.TimeDelivered,
                                       Height = mails.Height,
                                       Width = mails.Width,
                                       Hight = mails.Hight,
                                       Weight = mails.Weight,
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
                          ).Where(m => m.CurrentStatus.StatusCatalogId != deliveredStateId).ToListAsync();

            ViewBag.StatusesList = statuses;

            return _context.Mail != null ?
                        View(mailsList) :
                        Problem("Entity set 'ApplicationDbContext.Mail'  is null.");
        }

        // GET: Carrier/Mail/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Mail == null)
            {
                return NotFound();
            }

            var mail = await (from mails in _context.Mail
                              where mails.Id == id
                              join statuses in _context.MailStatus
                              on mails.Id equals statuses.MailId
                              orderby statuses.TimeAssigned descending
                              join startLocations in _context.Location
                              on mails.StartLocationId equals startLocations.Id
                              join endLocations in _context.Location
                              on mails.EndLocationId equals endLocations.Id
                              select new Mail()
                              {
                                  Id = mails.Id,
                                  Code = mails.Code,
                                  RecipientAddress = mails.RecipientAddress,
                                  SenderAddress = mails.SenderAddress,
                                  StartLocationId = mails.StartLocationId,
                                  EndLocationId = mails.EndLocationId,
                                  StartLocationName = startLocations.Name,
                                  EndLocationName = endLocations.Name,
                                  CategoryId = mails.CategoryId,
                                  TimeInserted = mails.TimeInserted,
                                  Height = mails.Height,
                                  Width = mails.Width,
                                  Weight = mails.Weight,
                                  Hight = mails.Weight,
                                  CurrentStatus = new MailStatus()
                                  {
                                      Id = statuses.Id,
                                      MailId = mails.Id,
                                      StatusCatalogId = statuses.StatusCatalogId,
                                      TimeAssigned = statuses.TimeAssigned
                                  }
                              })
            .FirstOrDefaultAsync();


            if (mail == null)
            {
                return NotFound();
            }
            ViewBag.StatusesCatalog = await _context.StatusCatalog.ToListAsync();

            List<MailStatus> list = await _helper.GetMailHistory(mail);
            ViewBag.MailStatuses = new StatusHistoryModel() { Statuses = list };

            ViewBag.PackageItems = await _context.Mail.Where(m => m.MailBundleId == mail.Id).ToListAsync();

            return View(mail);
        }

        // GET: Carrier/Mail/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
            var startLocationId = user?.LocationAssignedId.HasValue == true ? (int)user?.LocationAssignedId.Value : 0;
            var endLocationsList = (await _context.Location.Where(loc => loc.Id != startLocationId).ToListAsync()).ConvertToSelectList<Location>(-1);

            ViewBag.EndLocationsList = endLocationsList;
            ViewBag.StatusesCatalog = await _context.StatusCatalog.ToListAsync();

            Mail newMail = new Mail()
            {
                StartLocationId = startLocationId,
            };
            if (endLocationsList.Count > 0)
                newMail.EndLocationId = Int32.Parse(endLocationsList.First().Value);

            return View(newMail);
        }

        // POST: Carrier/Mail/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MailBundleId,Code,RecipientAddress,SenderAddress,StartLocationId,EndLocationId,CategoryId,TimeInserted,TimeDelivered,CreatedById,ModifiedById,Height,Width,Hight,Weight,ReceiverContactNr")] Mail mail)
        {
            if (mail != null)
            {
                mail.CategoryId = (await _context.MailCategory.FirstOrDefaultAsync(category => String.Equals(category.Name, Globals.BUNDLE_CATEGORY))).Id;
                mail.RecipientAddress = "Location of postal office network";
                mail.TimeInserted = DateTime.UtcNow;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                mail.CreatedById = userId;
                mail.ReceiverContactNr = "+355696968234";
                mail.Code = Guid.NewGuid();

                _context.Add(mail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(mail);
        }

        // GET: Carrier/Mail/Edit/5
        public async Task<IActionResult> Edit(int? id)
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
            ViewBag.StatusesCatalog = await _context.StatusCatalog.ToListAsync();

            return View(mail);
        }

        // POST: Carrier/Mail/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MailBundleId,Code,RecipientAddress,SenderAddress,StartLocationId,EndLocationId,CategoryId,TimeInserted,TimeDelivered,CreatedById,ModifiedById,Height,Width,Hight,Weight,ReceiverContactNr")] Mail mail)
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

        // GET: Carrier/Mail/ManageItems/5
        public async Task<IActionResult> ManageItems(int? id)
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

            // consider rewriting the query
            var statusesList = await _context.StatusCatalog.ToListAsync();
            var registeredStatusRec = statusesList.FirstOrDefault(st => String.Equals(st.Name, Globals.REGISTERED));
            var pickUpStatusRec = statusesList.FirstOrDefault(st => String.Equals(st.Name, Globals.PICK_UP));
            var deliveredToTransitCenter = statusesList.FirstOrDefault(st => String.Equals(st.Name, Globals.DELIVERED_TO_TRANSIT_CENTER));
            var newMails = await (from mails in _context.Mail
                                  where mails.Id != id && (mails.MailBundleId == null || mails.MailBundleId == id)
                                  join categories in _context.MailCategory
                                  on mails.CategoryId equals categories.Id
                                  join locations in _context.Location
                                  on mails.EndLocationId equals locations.Id
                                  select new Mail()
                                  {
                                      Id = mails.Id,
                                      MailBundleId = mails.MailBundleId,
                                      Code = mails.Code,
                                      RecipientAddress = mails.RecipientAddress,
                                      SenderAddress = mails.SenderAddress,
                                      StartLocationId = mails.StartLocationId,
                                      EndLocationId = mails.EndLocationId,
                                      EndLocationName = locations.Name,
                                      CategoryId = mails.CategoryId,
                                      CurrentStatus = (from statuses in _context.MailStatus
                                                       where statuses.MailId == mails.Id
                                                       join statusCatalogs in _context.StatusCatalog
                                                       on statuses.StatusCatalogId equals statusCatalogs.Id
                                                       orderby statuses.TimeAssigned descending
                                                       select new MailStatus()
                                                       {
                                                           Id = statuses.Id,
                                                           MailId = mails.Id,
                                                           StatusCatalogId = statuses.StatusCatalogId,
                                                           StatusName = statusCatalogs.Name,
                                                           TimeAssigned = statuses.TimeAssigned
                                                       }).FirstOrDefault(),
                                      Category = new MailCategory()
                                      {
                                          Id = categories.Id,
                                          Name = categories.Name
                                      }
                                  })
                                  .Where(m => m.CurrentStatus.StatusCatalogId == registeredStatusRec.Id || m.CurrentStatus.StatusCatalogId == pickUpStatusRec.Id || m.CurrentStatus.StatusCatalogId == deliveredToTransitCenter.Id).ToListAsync();


            ViewBag.PackageItemsList = new PackageModel()
            {
                Bundle = mail,
                PackageItems = newMails
            };


            return View(mail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddItem([Bind("Id,MailBundleId")] Mail mail)
        {
            if (mail.Id != 0 && mail.MailBundleId != 0)
            {
                try
                {
                    Mail mailToUpdate = await _context.Mail.FirstOrDefaultAsync(m => m.Id == mail.Id);
                    if (mailToUpdate != null)
                    {
                        mailToUpdate.MailBundleId = mail.MailBundleId;
                        mailToUpdate.ModifiedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        _context.Update(mailToUpdate);

                        await _context.SaveChangesAsync();
                        await NotifyUserMailStatusChanged(mail.Id, Globals.PICK_UP);
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
            }

            return RedirectToAction(nameof(ManageItems), new { id = mail.MailBundleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem([Bind("Id,MailBundleId")] Mail mail)
        {
            if (mail.Id != 0)
            {
                try
                {
                    Mail mailToUpdate = await _context.Mail.FirstOrDefaultAsync(m => m.Id == mail.Id);
                    if (mailToUpdate != null)
                    {
                        // when removed from package. Empty the bundleid
                        mailToUpdate.MailBundleId = null;
                        mailToUpdate.ModifiedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        _context.Update(mailToUpdate);

                        // create new status to registered for mail
                        var newMailStatusRegistered = new MailStatus()
                        {
                            MailId = mailToUpdate.Id,
                            OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            StatusCatalogId = (await _context.StatusCatalog.FirstOrDefaultAsync(s => String.Equals(s.Name, Globals.REGISTERED))).Id,
                            TimeAssigned = DateTime.UtcNow
                        };
                        _context.Add(newMailStatusRegistered);

                        await _context.SaveChangesAsync();
                        await NotifyUserMailStatusChanged(mail.Id, Globals.REGISTERED);
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
            }

            return RedirectToAction(nameof(ManageItems), new { id = mail.MailBundleId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDelivered([Bind("Id")] Mail package)
        {
            if (package.Id != 0)
            {
                try
                {
                    Mail packageToUpdate = await _context.Mail.FirstOrDefaultAsync(m => m.Id == package.Id);
                    if (packageToUpdate != null)
                    {
                        // when arriving at postoffice center. Its going to be striped by the package. Empty the bundleid
                        //packageToUpdate.MailBundleId = null;
                        //packageToUpdate.ModifiedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        //_context.Update(packageToUpdate);
                        // if bundle destination is equal with package destiantion
                        // set status -- package arrived at destination center (post/office)
                        // or set status -- package arrive at transite location -- maybe use note field for this info extra (location name)

                        var mailStatuses = await _context.StatusCatalog.ToListAsync();

                        // create new status to registered for mail
                        var packageStatusToDelivered = new MailStatus()
                        {
                            MailId = packageToUpdate.Id,
                            OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            TimeAssigned = DateTime.UtcNow,
                            StatusCatalogId = mailStatuses.Find(s => String.Equals(s.Name, Globals.DELIVERED)).Id
                        };
                        _context.Add(packageStatusToDelivered);

                        var packageItems = await _context.Mail.Where(m => m.MailBundleId == packageToUpdate.Id).ToListAsync();

                        foreach (var item in packageItems)
                        {
                            // remove items from package
                            item.MailBundleId = null;
                            item.ModifiedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                            _context.Update(item);

                            // create new status for item
                            var itemNewStatus = new MailStatus()
                            {
                                MailId = item.Id,
                                OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                                TimeAssigned = DateTime.UtcNow
                            };

                            // if center where package was delivered was the same as the center destination of mail
                            string deliveredStatus = Globals.DELIVERED_TO_TRANSIT_CENTER;
                            if (packageToUpdate.EndLocationId == item.EndLocationId)
                                deliveredStatus = Globals.DELIVERED_TO_DESTINATION_CENTER;

                            itemNewStatus.StatusCatalogId = mailStatuses.Find(s => String.Equals(s.Name, deliveredStatus)).Id;
                            _context.Add(itemNewStatus);

                            await _context.SaveChangesAsync();
                            await NotifyUserMailStatusChanged(item.Id, deliveredStatus);
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MailExists(package.Id))
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


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetInTransit([Bind("Id")] Mail package)
        {
            if (package.Id != 0)
            {
                try
                {
                    Mail packageToUpdate = await _context.Mail.FirstOrDefaultAsync(m => m.Id == package.Id);
                    if (packageToUpdate != null)
                    {
                        // when arriving at postoffice center. Its going to be striped by the package. Empty the bundleid
                        //packageToUpdate.MailBundleId = null;
                        //packageToUpdate.ModifiedById = User.FindFirstValue(ClaimTypes.NameIdentifier);
                        //_context.Update(packageToUpdate);
                        // if bundle destination is equal with package destiantion
                        // set status -- package arrived at destination center (post/office)
                        // or set status -- package arrive at transite location -- maybe use note field for this info extra (location name)

                        var mailStatuses = await _context.StatusCatalog.ToListAsync();

                        // create new status to registered for mail
                        var packageStatusToDelivered = new MailStatus()
                        {
                            MailId = packageToUpdate.Id,
                            OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                            TimeAssigned = DateTime.UtcNow,
                            StatusCatalogId = mailStatuses.Find(s => String.Equals(s.Name, Globals.IN_TRANSIT)).Id
                        };
                        _context.Add(packageStatusToDelivered);

                        var packageItems = await _context.Mail.Where(m => m.MailBundleId == packageToUpdate.Id).ToListAsync();

                        foreach (var item in packageItems)
                        {
                            // create new status for item
                            var itemNewStatus = new MailStatus()
                            {
                                MailId = item.Id,
                                OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                                TimeAssigned = DateTime.UtcNow,
                                StatusCatalogId = mailStatuses.Find(s => String.Equals(s.Name, Globals.IN_TRANSIT)).Id
                            };


                            _context.Add(itemNewStatus);

                            await _context.SaveChangesAsync();
                            await NotifyUserMailStatusChanged(item.Id, Globals.IN_TRANSIT);
                        }

                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MailExists(package.Id))
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

        // GET: Carrier/Mail/Delete/5
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

        // POST: Carrier/Mail/Delete/5
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
    }
}
