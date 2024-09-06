using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using Org.BouncyCastle.Utilities;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using QuikGraph;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PostalManagementMVC.Utilities
{
    public class Helper
    {
        private ApplicationDbContext _context;

        public Helper(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<MailStatus>> GetMailHistory(Mail? mail)
        {
            return await (from mailStatus in _context.MailStatus
                          where mailStatus.MailId == mail.Id
                          join statusCatalog in _context.StatusCatalog
                          on mailStatus.StatusCatalogId equals statusCatalog.Id
                          join user in _context.Users
                          on mailStatus.OwnerId equals user.Id
                          select new MailStatus
                          {
                              Id = mailStatus.Id,
                              MailId = mailStatus.MailId,
                              StatusName = statusCatalog.Name,
                              StatusCatalogId = mailStatus.StatusCatalogId,
                              OwnerId = mailStatus.OwnerId,
                              Note = mailStatus.Note,
                              OwnerName = user.NormalizedUserName,
                              TimeAssigned = mailStatus.TimeAssigned,
                          }
            ).ToListAsync();
        }

        public async Task<ApplicationUser?> GetUserWithLocationById(string id)
        {
            return await (from users in _context.Users
                          where users.Id == id
                          join locations in _context.Location
                          on users.LocationAssignedId equals locations.Id
                          select new ApplicationUser()
                          {
                              Id = users.Id,
                              FirstName = users.FirstName,
                              LastName = users.LastName,
                              UserName = users.UserName,
                              NormalizedEmail = users.NormalizedEmail,
                              NormalizedUserName = users.NormalizedUserName,
                              LocationAssignedId = locations.Id,
                              LocationAssignedName = locations.Name,
                              ActiveFrom = users.ActiveFrom,
                              ActiveTo = users.ActiveTo,
                          }
                               ).FirstOrDefaultAsync();
        }

        public async Task<List<MailStatus>> GetMailStatusesByCode(Mail? mail)
        {
            if (mail == null || Guid.TryParse(mail.Code?.ToString(), out Guid res))
                return new List<MailStatus>();

            return await (from mails in _context.Mail
                          where mails.Code == mail.Code
                          join mailStatus in _context.MailStatus
                          on mails.Id equals mailStatus.MailId
                          join statusCatalog in _context.StatusCatalog
                          on mailStatus.StatusCatalogId equals statusCatalog.Id
                          join user in _context.Users
                          on mailStatus.OwnerId equals user.Id
                          select new MailStatus
                          {
                              Id = mailStatus.Id,
                              MailId = mailStatus.MailId,
                              StatusName = statusCatalog.Name,
                              StatusCatalogId = mailStatus.StatusCatalogId,
                              OwnerId = mailStatus.OwnerId,
                              OwnerName = user.NormalizedUserName,
                              TimeAssigned = mailStatus.TimeAssigned,
                          }
            ).ToListAsync();
        }

        public async Task<Mail?> GetMailWithCurrentStatus(int id)
        {
            return await (from mails in _context.Mail
                          where mails.Id == id
                          join statuses in _context.MailStatus
                          on mails.Id equals statuses.MailId
                          orderby statuses.TimeAssigned descending
                          select new Mail()
                          {
                              Id = mails.Id,
                              Code = mails.Code,
                              RecipientAddress = mails.RecipientAddress,
                              SenderAddress = mails.SenderAddress,
                              StartLocationId = mails.StartLocationId,
                              EndLocationId = mails.EndLocationId,
                              CategoryId = mails.CategoryId,
                              PostalCodeId = mails.PostalCodeId,
                              BundleCode = mails.BundleCode,
                              MailBundleId = mails.MailBundleId,
                              TimeInserted = mails.TimeInserted,
                              TimeDelivered = mails.TimeDelivered,
                              CreatedById = mails.CreatedById,
                              ModifiedById = mails.ModifiedById,
                              Height = mails.Height,
                              Width = mails.Width,
                              Hight = mails.Hight,
                              Weight = mails.Weight,
                              DaysToDelivery = mails.DaysToDelivery,
                              ReceiverContactNr = mails.ReceiverContactNr,
                              ChoosenPath = mails.ChoosenPath,
                              CurrentStatus = new MailStatus()
                              {
                                  Id = statuses.Id,
                                  MailId = mails.Id,
                                  StatusCatalogId = statuses.StatusCatalogId,
                                  TimeAssigned = statuses.TimeAssigned
                              }
                          })
            .FirstOrDefaultAsync();
        }

      

        public async Task<LocationConnection?> GetLocationConn(int? id)
        {
            return await (from conns in _context.LocationConnection
                          where conns.Id == id
                          join startLocations in _context.Location
                          on conns.FromLocationId equals startLocations.Id
                          join endLocations in _context.Location
                          on conns.ToLocationId equals endLocations.Id
                          select new LocationConnection()
                          {
                              Id = conns.Id,
                              FromLocationId = conns.FromLocationId,
                              ToLocationId = conns.ToLocationId,
                              TransportDays = conns.TransportDays,
                              Cost = conns.Cost,
                              OnDay = conns.OnDay,
                              FromLocationName = startLocations.Name,
                              ToLocationName = endLocations.Name,
                          }
                                                   ).FirstOrDefaultAsync();
        }

        public async Task<PostalCode?> GetPostalCodeById(int? id)
        {
            return await (from zipcode in _context.PostalCode
                          where zipcode.Id == id
                          join locations in _context.Location
                          on zipcode.LocationId equals locations.Id
                          select new PostalCode()
                          {
                              Id = zipcode.Id,
                              ZipCode = zipcode.ZipCode,
                              LocationId = locations.Id,
                              LocationName = locations.Name
                          }
                            ).FirstOrDefaultAsync();
        }


        public static bool IsValidPhoneNumber(string phone)
        {
            if (String.IsNullOrWhiteSpace(phone))
                return false;

            try
            {
                return Regex.IsMatch(phone, @"^\+?\d{10,15}$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();

                    // Pull out and process domain name (throws ArgumentException on invalid)
                    string domainName = idn.GetAscii(match.Groups[2].Value);

                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static List<SelectListItem> GetWeekDaysDropdownElements()
        {
            List<SelectListItem> weekDaysList = new List<SelectListItem>();

            foreach (var day in Globals.WEEK_DAYS_ARR)
            {
                weekDaysList.Add(new SelectListItem()
                {
                    Text = day,
                    Value = day,
                });
            }

            return weekDaysList;
        }

        public static string ConcatAllExceptionMessages(Exception ex)
        {
            if (ex == null) return String.Empty;

            string errorMess = String.Empty;
            Exception currentEx = ex;
            do
            {
                errorMess += currentEx.Message;
                currentEx = currentEx.InnerException;
            }
            while (currentEx != null);

            return errorMess;
        }

        public static string LocationConnCreationCustomErrorMessageHandler(Exception ex)
        {
            string errorMess = String.Empty;
            string operationsExceptions = ConcatAllExceptionMessages(ex);

            if (operationsExceptions?.Contains("IX_OnlyOneConnectionForDayForLocations") == true)
                errorMess = "An connection for these locations and this day is already created.";
            else if (operationsExceptions?.Contains("CK_ValidWeekDay") == true)
                errorMess = "The day of week inserted is not valid. The allowed values are as following: 'MONDAY','TUESDAY','WEDNESDAY','THURSDAY','FRIDAY','SATURDAY','SUNDAY'";
            else if (operationsExceptions?.Contains("CK_CostValid") == true)
                errorMess = "The cost value is out of range. Please provide an value between 0 and 10000. Where 0 is included.";
            else if (operationsExceptions?.Contains("CK_FromToLocationsNotEqual") == true)
                errorMess = "You can create an connection only between diffferent locations.";

            return errorMess;
        }

        public static string PostalCodeCreationCustomErrorMessageHandler(Exception ex)
        {
            string errorMess = String.Empty;
            string operationsExceptions = ConcatAllExceptionMessages(ex);

            if (operationsExceptions?.Contains("IX_ZipCodeUnique") == true)
                errorMess = "The zip code is existing. You cannot associate it with another locations.";

            return errorMess;
        }

        public static int GetDayIndex(string day)
        {
            return Array.FindIndex(Globals.WEEK_DAYS_ARR, d => String.Equals(d, day, StringComparison.OrdinalIgnoreCase));
        }

        public static void GetFoundPaths(int target, TryFunc<int, IEnumerable<Edge<int>>> tryGetPaths, List<IEnumerable<Edge<int>>> foundPaths)
        {
            if (tryGetPaths(target, out IEnumerable<Edge<int>> path) && !ContainsPath(foundPaths, path))
            {
                foundPaths.Add(path);
            }
        }

        public static bool ContainsPath(List<IEnumerable<Edge<int>>> paths, IEnumerable<Edge<int>> newPath)
        {
            if (paths == null || newPath == null || paths.Count == 0)
                return false;

            foreach (var path in paths)
            {
                if (path.Count() != newPath.Count())
                    continue;

                var i = 0;
                for (; i < path.Count(); i++)
                {
                    var existingEdge = path.ElementAtOrDefault(i);
                    var newEdge = newPath.ElementAtOrDefault(i);

                    if ((existingEdge == null && newEdge != null) || (existingEdge != null && newEdge == null) || ((existingEdge != null && newEdge != null) && (existingEdge.Source != newEdge.Source || existingEdge.Target != newEdge.Target)))
                        break;
                }

                if (i == path.Count())
                    return true;
            }

            return false;
        }
        public static string ConvertEdgesToLocationsNames(double monetaryCost, double timeCost, IEnumerable<Edge<int>> path, List<Entities.Location> locations)
        {
            string concatedPath = $"(Time: {timeCost}days, Cost: {monetaryCost}Lek) ";
            bool isFirst = true;

            foreach (var edge in path)
            {
                if (isFirst)
                {
                    isFirst = false;
                    concatedPath += (locations.FirstOrDefault(l => l.Id == edge.Source)?.Name ?? "NO NAME") + " -> " + (locations.FirstOrDefault(l => l.Id == edge.Target)?.Name ?? "NO NAME");
                }
                else
                {
                    concatedPath += " -> " + (locations.FirstOrDefault(l => l.Id == edge.Target)?.Name ?? "NO NAME");
                }
            }

            return concatedPath;
        }

        internal static double GetDaysToDeliveryFromChoosenPath(string? choosenPath)
        {
            if (choosenPath == null) return 0;

            var detailTimeSubStr = choosenPath.Split("days");
            if (detailTimeSubStr.Length > 1)
            {
                var daysStr = detailTimeSubStr.First().Split("Time: ").Last();
                if (Int32.TryParse(daysStr, out int days))
                    return days;
            }

            return 0;
        }

        internal static double GetCostFromChoosenPath(string? choosenPath)
        {
            if (choosenPath == null) return 0;

            var detailCostSubStr = choosenPath.Split("Lek");
            if (detailCostSubStr.Length > 1)
            {
                var costStr = detailCostSubStr.First().Split("Cost: ").Last();
                if (Int32.TryParse(costStr, out int cost))
                    return cost;
            }

            return 0;
        }

        internal Dictionary<Edge<int>, double> GetLocationsConnectionWeights(int source, List<LocationConnection> locationConns, int initialDay, List<Edge<int>> edges, Func<List<LocationConnection>, int, ValueTuple<bool, double>> FuncMinWeightConn)
        {
            Dictionary<Edge<int>, double> weights = new Dictionary<Edge<int>, double>();
            List<Edge<int>> processedNodes = new List<Edge<int>>();

            var connectedNodes = locationConns.Where(c => c.FromLocationId == source)
                                              .DistinctBy(x => x.ToLocationId)
                                              .Select(e => new Edge<int>(e.FromLocationId, e.ToLocationId))
                                              .ToArray();

            if (connectedNodes == null || connectedNodes.Length == 0)
                return weights;

            GetEdgeWeightRecursive(source, locationConns, connectedNodes, 0, initialDay, weights, processedNodes, edges, FuncMinWeightConn);

            return weights;
        }

        public void GetEdgeWeightRecursive(int source, List<LocationConnection> allConns, Edge<int>[] connectedNodes, int connectedNodeIndex, int startingDay, Dictionary<Edge<int>, double> weights, List<Edge<int>> processedNodes, List<Edge<int>> edges, Func<List<LocationConnection>, int, ValueTuple<bool, double>> FuncMinWeightConn)
        {
            if (connectedNodeIndex >= connectedNodes.Length)
                return;

            if (processedNodes.FirstOrDefault(c => c.Source == source && c.Target == connectedNodes[connectedNodeIndex].Target) != null)
                return;

            // between two nodes are multiple connections
            // i.e.: From Tirana to Durresi are deliveries at Monday, Tuesday and Thurday
            var allConnectionsSourceTarget = allConns.Where(c => c.FromLocationId == source && c.ToLocationId == connectedNodes[connectedNodeIndex].Target)
                .ToList();


            double cost = 0;
            int arrivalDayIndex = 0;
            if (allConnectionsSourceTarget.Count > 0)
            {
                var lowestCostConn = FuncMinWeightConn(allConnectionsSourceTarget, startingDay);
                bool isSuccess = lowestCostConn.Item1;
                if (isSuccess)
                {
                    cost = lowestCostConn.Item2;

                    // arrivalday is valid only for time weight processing
                    // discarded in monetary cost calculations
                    arrivalDayIndex = (startingDay + (int)cost) % 7;

                    var edge = edges.FirstOrDefault(e => e.Source == connectedNodes[connectedNodeIndex].Source && e.Target == connectedNodes[connectedNodeIndex].Target);

                    if(edge != null)
                        weights.Add(edge, cost);
                }
            }

            processedNodes.Add(connectedNodes[connectedNodeIndex]);

            // get childs of the target
            var connectedNodesWithCurrentTarget = allConns.Where(c => c.FromLocationId == connectedNodes[connectedNodeIndex].Target)
                                          .DistinctBy(x => x.ToLocationId)
                                          .Select(e => new Edge<int>(e.FromLocationId, e.ToLocationId))
                                          .ToArray();

            if (connectedNodesWithCurrentTarget != null && connectedNodesWithCurrentTarget.Length > 0)
                GetEdgeWeightRecursive(connectedNodes[connectedNodeIndex].Target, allConns, connectedNodesWithCurrentTarget, 0, arrivalDayIndex, weights, processedNodes, edges, FuncMinWeightConn);


            if (connectedNodeIndex < connectedNodes.Length - 1)
                GetEdgeWeightRecursive(source, allConns, connectedNodes, connectedNodeIndex + 1, startingDay, weights, processedNodes, edges, FuncMinWeightConn);
        }

        public static int NumberOfDaysBetweenDaysOfWeek(int initialDay, int arrivalDay)
        {
            // we are considering that the mail can be send to another destination on the preceding day
            // so if the mail is registered on the monday. It's going to be delivered no sonner than tuesday
            // if mail arrives at post office location on the monday. And the deliveries from this location
            // to destination location happens only on the mondays. 
            // the following mail is going to wait a week to be sent
            if (initialDay < arrivalDay)
                return Math.Abs(initialDay - arrivalDay);
            else
                return Math.Abs(initialDay - arrivalDay - 7);

        }

        public static ValueTuple<bool, double> GetConnectionWithLowestTimeCost(List<LocationConnection> conns, int startingDay)
        {
            if (conns == null || conns.Count == 0)
                return (false, 0);

            // number of days we are going to add for delivery of package by distributor
            int DELIVERY_DAYS_COST = 1;

            conns.ForEach(c => {
                c.OnDayIndex = GetDayIndex(c.OnDay);
                c.TimeCost = NumberOfDaysBetweenDaysOfWeek(startingDay, c.OnDayIndex) + c.TransportDays + DELIVERY_DAYS_COST; 
            });

            var minConn = conns.OrderBy(x => x.TimeCost).FirstOrDefault();

            if(minConn == null)
                return (false, 0);

            return (true, minConn.TimeCost);
        }

        public static ValueTuple<bool, double> GetConnectionWithLowestMonetaryCost(List<LocationConnection> conns, int startingDay)
        {
            if (conns == null || conns.Count == 0)
                return (false, 0);

            var minConn = conns.OrderBy(x => x.Cost).FirstOrDefault();

            if (minConn == null)
                return (false, 0);

            return (true, minConn.Cost);
        }
    }
}
