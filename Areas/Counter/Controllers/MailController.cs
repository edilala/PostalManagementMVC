using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Areas.Counter.Models;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Extensions;
using PostalManagementMVC.Interfaces;
using PostalManagementMVC.Models;
using PostalManagementMVC.Utilities;
using QuikGraph;
using QuikGraph.Algorithms;
using System.Security.Claims;

namespace PostalManagementMVC.Areas.Counter.Controllers
{
    [Area("Counter")]
    [Authorize(Roles = "Admin,Counter")]
    public class MailController : AbstractController
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public MailController(ApplicationDbContext context, IEmailSender emailSender, UserManager<ApplicationUser> userManager) : base(context, emailSender) {
            _userManager = userManager;
        }

        // GET: Counter/Mail
        public async Task<IActionResult> Index()
        {
            var categoryId = (await _context.MailCategory.FirstOrDefaultAsync(category => String.Equals(category.Name, Globals.BUNDLE_CATEGORY))).Id;

            return _context.Mail != null ?
                          View(await _context.Mail.Where(mail => mail.CategoryId != categoryId).ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Mail'  is null.");
        }


        // GET: Counter/Mail/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Mail == null)
            {
                return NotFound();
            }

            var mail = await _context.Mail.FirstOrDefaultAsync(m => m.Id == id);
            if (mail == null)
            {
                return NotFound();
            }

            // optimization
            // retrieve all informations below in only an query

            ViewBag.StartLocationName = (await _context.Location.FirstOrDefaultAsync(m => m.Id == mail.StartLocationId))?.Name;
            ViewBag.EndLocationName = (await _context.Location.FirstOrDefaultAsync(m => m.Id == mail.EndLocationId))?.Name;
            var mailCategory = await _context.MailCategory.FirstOrDefaultAsync(m => m.Id == mail.CategoryId);
            ViewBag.CategoryName = mailCategory?.Name;
            ViewBag.TransportFee = mailCategory?.Fee;
            ViewBag.TotalCost = mailCategory?.Fee + Helper.GetCostFromChoosenPath(mail.ChoosenPath);

            if (mail.PostalCodeId.HasValue)
                ViewBag.PostalCode = (await _context.PostalCode.FirstOrDefaultAsync(m => m.Id == mail.PostalCodeId)).ZipCode;

            List<MailStatus> mailsStatusesInTime = await _helper.GetMailHistory(mail);
            ViewBag.StatusesList = new StatusHistoryModel() { Statuses = mailsStatusesInTime };
            ViewBag.CurrentStatus = mailsStatusesInTime.OrderByDescending(item => item.TimeAssigned).FirstOrDefault();


            return View(mail);
        }



        // GET: Counter/Mail/Create
        public async Task<IActionResult> Create()
        {
            var formDropboxes = await GetCounterFormRelatedDropbox();

            int startLocationId = formDropboxes.Item1;
            ViewBag.EndLocationsList = formDropboxes.Item2;
            ViewBag.CategoriesList = formDropboxes.Item3;
            ViewBag.PostalCodesList = formDropboxes.Item4;
            ViewBag.ChoosenPath = formDropboxes.Item5;

            Mail newMail = new Mail()
            {
                StartLocationId = startLocationId,
            };

            return View(newMail);
        }


        // POST: Counter/Mail/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MailBundleId,Code,CategoryId,RecipientAddress,SenderAddress,StartLocationId,EndLocationId,TimeInserted,TimeDelivered,CreatedById,ReceiverContactNr,PostalCodeId,ChoosenPath,Height,Width,Hight,Weight")] Mail mail)
        {
            mail.TimeInserted = DateTime.UtcNow;


            if (ModelState.IsValid)
            {
                if (!Helper.IsValidPhoneNumber(mail.ReceiverContactNr))
                {
                    ModelState.AddModelError("ReceiverContactNr", "The phone number format is invalid");

                    var formDropboxes = await GetCounterFormRelatedDropbox();

                    ViewBag.EndLocationsList = formDropboxes.Item2;
                    ViewBag.CategoriesList = formDropboxes.Item3;
                    ViewBag.PostalCodesList = formDropboxes.Item4;
                    ViewBag.ChoosenPath = formDropboxes.Item5;

                    return View(mail);
                }

                mail.DaysToDelivery = Helper.GetDaysToDeliveryFromChoosenPath(mail.ChoosenPath);

                mail.Code = Guid.NewGuid();

                _context.Add(mail);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(mail);
        }

        // GET: Counter/Mail/Edit/5
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
            var postalCodes = await _context.PostalCode.ToListAsync();
            ViewBag.PostalCodesList = postalCodes.ConvertPostalCodesToSelectList(-1);
            return View(mail);
        }

        // POST: Counter/Mail/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MailBundleId,Code,RecipientAddress,SenderAddress,StartLocationId,EndLocationId,TimeInserted,TimeDelivered")] Mail mail)
        {
            if (id != mail.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!Helper.IsValidPhoneNumber(mail.ReceiverContactNr))
                    {
                        ModelState.AddModelError("ContactDetails", "The phone number format is invalid");

                        var formDropboxes = await GetCounterFormRelatedDropbox();

                        ViewBag.EndLocationsList = formDropboxes.Item2;
                        ViewBag.CategoriesList = formDropboxes.Item3;
                        ViewBag.PostalCodesList = formDropboxes.Item4;
                        ViewBag.ChoosenPath = formDropboxes.Item5;

                        return View(mail);
                    }

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

        // GET: Counter/Mail/Delete/5
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

        // POST: Counter/Mail/Delete/5
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

        // GET: Counter/Mail/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
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

        // POST: Counter/Mail/Delete/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
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
                var registeredStatusRec = await _context.StatusCatalog.FirstOrDefaultAsync(st => String.Equals(st.Name, Globals.REGISTERED));

                // only mails in registered status can be cancelled
                if (registeredStatusRec.Id != mail.CurrentStatus.StatusCatalogId)
                {
                    return RedirectToAction(nameof(Index));
                }
                var cancelStatusRec = await _context.StatusCatalog.FirstOrDefaultAsync(st => String.Equals(st.Name, Globals.CANCELLED));
                if (cancelStatusRec != null)
                {
                    MailStatus newStatus = new MailStatus()
                    {
                        MailId = id,
                        StatusCatalogId = cancelStatusRec.Id,
                        TimeAssigned = DateTime.UtcNow,
                        OwnerId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    };

                    _context.Add(newStatus);
                }

                await _context.SaveChangesAsync();
                await NotifyUserMailStatusChanged(mail.Id, Globals.CANCELLED);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<List<SelectListItem>> RelatedPostalCodes(int locationId)
        {
            return (await _context.PostalCode.Where(p => p.LocationId == locationId).ToListAsync()).ConvertPostalCodesToSelectList(-1);
        }



        public async Task<List<SelectListItem>> DeliveryPath(int source, int target)
        {
            List<SelectListItem> noPaths = new List<SelectListItem>();
            noPaths.Add(new SelectListItem()
            {
                Text = "NO PATH COULD BE FOUND",
                Value = "NO PATH COULD BE FOUND",
            });

            if (source == 0 || target == 0 || source == target)
                return noPaths;

            var startLoc = await _context.Location.FirstOrDefaultAsync(l => l.Id == source);
            if (startLoc == null)
                return noPaths;

            var endLoc = await _context.Location.FirstOrDefaultAsync(l => l.Id == target);
            if (endLoc == null)
                return noPaths;

            var allConns = await _context.LocationConnection.ToListAsync();

            var uniqueConns = allConns.Concat(new List<LocationConnection>())
                .DistinctBy(x => new { x.FromLocationId, x.ToLocationId })
                .ToList();
            // set onday index for each item
            uniqueConns.ForEach(c => c.OnDayIndex = Helper.GetDayIndex(c.OnDay));

            if (uniqueConns.Count == 0)
                return noPaths;

            var edges = uniqueConns.Select(e => new Edge<int>(e.FromLocationId, e.ToLocationId)).ToList();
            var graph = edges.ToBidirectionalGraph<int, Edge<int>>().ToArrayBidirectionalGraph();


            // solve by time
            int currentDay = Helper.GetDayIndex(Enum.GetName<DayOfWeek>(DateTime.Now.DayOfWeek).ToUpper());

            var _timeWeights = _helper.GetLocationsConnectionWeights(source, allConns, currentDay, edges, Helper.GetConnectionWithLowestTimeCost);
            var _costWeights = _helper.GetLocationsConnectionWeights(source, allConns, currentDay, edges, Helper.GetConnectionWithLowestMonetaryCost);

            Func<Edge<int>, double> _costWeightDelegate = AlgorithmExtensions.GetIndexer(_costWeights);
            Func<Edge<int>, double> _timeWeightDelegate = AlgorithmExtensions.GetIndexer(_timeWeights);

            // Compute shortest paths
            TryFunc<int, IEnumerable<Edge<int>>> tryGetPathsByCost = graph.ShortestPathsDijkstra(_costWeightDelegate, source);
            TryFunc<int, IEnumerable<Edge<int>>> tryGetPathsByTime = graph.ShortestPathsDijkstra(_timeWeightDelegate, source);

            List<IEnumerable<Edge<int>>> edgesPaths = new List<IEnumerable<Edge<int>>>();

            // Get path for given vertices
            Helper.GetFoundPaths(target, tryGetPathsByCost, edgesPaths);
            Helper.GetFoundPaths(target, tryGetPathsByTime, edgesPaths);

            List<SelectListItem> pathsToChoose = new List<SelectListItem>();

            // when possible path found
            // display their names and costs
            if (edgesPaths.Count > 0)
            {
                var locations = await _context.Location.ToListAsync();

                foreach (var foundPath in edgesPaths)
                {
                    double timeCost = 0;
                    double monetaryCost = 0;
                    foreach (var edge in foundPath)
                    {
                        monetaryCost += _costWeightDelegate(edge);
                        timeCost += _timeWeightDelegate(edge);
                    }

                    // maybe neccessary to reorder edges before naming
                    // seen as issue in unidirectional graphs

                    string fullPathName = Helper.ConvertEdgesToLocationsNames(monetaryCost, timeCost, foundPath, locations);
                    pathsToChoose.Add(new SelectListItem()
                    {
                        Text = fullPathName,
                        Value = fullPathName
                    });
                }


                return pathsToChoose;
            }

            return noPaths;
        }

        [NonAction]
        public async Task<(int, List<SelectListItem>, List<SelectListItem>, List<SelectListItem>, List<SelectListItem>)> GetCounterFormRelatedDropbox()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(user => user.Id == userId);
            var startLocationId = user?.LocationAssignedId.HasValue == true ? (int)user?.LocationAssignedId.Value : 0;
            var endLocations = await _context.Location.Where(loc => loc.Id != startLocationId).ToListAsync();
            var endLocationsList = endLocations.ConvertToSelectList<Location>(-1);

            List<MailCategory> mailCategories = await _context.MailCategory.Where(c => c.Name != Globals.BUNDLE_CATEGORY).ToListAsync();
            var categoriesList = mailCategories.ConvertMailCategoriesToSelectList<MailCategory>(-1);

            var endLocationId = endLocations.First().Id;
            var postalCodes = await _context.PostalCode.Where(p => p.LocationId == endLocationId).ToListAsync();
            var postalCodesList = postalCodes.ConvertPostalCodesToSelectList(-1);

            var paths = await this.DeliveryPath(startLocationId, endLocationId);

            return ValueTuple.Create(startLocationId, endLocationsList, categoriesList, postalCodesList, paths);
        }


        private bool MailExists(int id)
        {
            return (_context.Mail?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        //
        // reports 
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

            List<Mail>? counterMailsInTimeWindow = null;

            bool isAdmin = await _userManager.IsInRoleAsync(currentUser, "Admin");

            if (isAdmin)
            {
                counterMailsInTimeWindow = await (from mails in _context.Mail
                                                  where mails.TimeInserted >= fromDate && mails.TimeInserted <= toDate
                                                  join users in _context.Users
                                                  on mails.CreatedById equals users.Id
                                                  select new Mail()
                                                  {
                                                      Id = mails.Id,
                                                      CreatedById = mails.CreatedById,
                                                      UserName = users.FirstName + " " + users.LastName,
                                                  }
                                  ).ToListAsync();

            }
            else
            {
                var userAssignedLocation = await _context.Location.FirstOrDefaultAsync(l => l.Id == currentUser.LocationAssignedId);
                if (userAssignedLocation != null)
                {
                    counterMailsInTimeWindow = await (from mails in _context.Mail
                                                      where mails.TimeInserted >= fromDate && mails.TimeInserted <= toDate
                                                      join users in _context.Users
                                                      on mails.CreatedById equals users.Id
                                                      join locations in _context.Location
                                                      on mails.StartLocationId equals locations.Id
                                                      join postOffices in _context.PostOffice
                                                      on locations.PostOfficeInChargeId equals postOffices.Id
                                                      where postOffices.Id == userAssignedLocation.PostOfficeInChargeId
                                                      select new Mail()
                                                      {
                                                          Id = mails.Id,
                                                          CreatedById = mails.CreatedById,
                                                          UserName = users.FirstName + " " + users.LastName,
                                                      }
                                      ).ToListAsync();
                }
            }

            var data = counterMailsInTimeWindow.GroupBy(x => x.CreatedById)
                            .Select(m => new KeyValuePair<string, double>(m.Select(ml => ml.UserName).FirstOrDefault(), m.Count())).OrderBy(x => x.Key).ToList();

            return data;
        }
    }
}
