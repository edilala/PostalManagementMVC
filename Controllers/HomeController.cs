using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Interfaces;
using PostalManagementMVC.Models;
using PostalManagementMVC.Utilities;
using System.Diagnostics;

namespace PostalManagementMVC.Controllers
{
    public class HomeController : AbstractController
    {
        public HomeController(ApplicationDbContext context, IEmailSender emailSender): base(context, emailSender) { }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public async Task<IActionResult> Track(TrackPageModel model)
        {
            // in order to not show the error code field is empty on first page load
            if(Request.Query.Count == 0)
                return View();
           

            if (String.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError("Code", "The code field is required!");
                return View();
            }

            if(!Guid.TryParse(model.Code, out var parsedCode)) {
                ModelState.AddModelError("Code", "The format of tracking number is not correct");
                return View();
            }

            var mail = await _context.Mail.FirstOrDefaultAsync(m => m.Code == parsedCode);
            if (mail == null)
            {
                ModelState.AddModelError("Code", "No mail with the following code exists");
                return View();
            }

            List<MailStatus> list = await _helper.GetMailHistory(mail);
            ViewBag.MailStatuses = new StatusHistoryModel() { Statuses = list };

            return View();
        }


        public async Task<IActionResult> Subscribe(TrackPageModel model)
        {
            model.Code = model.Code?.Trim();
            model.Email = model.Email?.Trim();
            ViewBag.MailSent = false;
            if(!Helper.IsValidEmail(model.Email))
                return RedirectToAction(nameof(Track));
            
            if (!Guid.TryParse(model.Code, out Guid parsedCode))
                return RedirectToAction(nameof(Track));

            var mail = await _context.Mail.FirstOrDefaultAsync(m => m.Code == parsedCode);

            if (mail == null)
                return RedirectToAction(nameof(Track));

            var subscription = await _context.ClientSubscription.FirstOrDefaultAsync(s => s.UserEmail == model.Email && s.TrackedMailId == mail.Id);

            if(subscription != null)
            {
                await _emailSender.SendEmailAsync(model.Email, "Postal Service Subscription Notification", "You have already subscribed to be notified for this product! Thank you!");
                ViewBag.MailSent = true;
                return RedirectToAction(nameof(Track));
            }
            ViewBag.MailSent = false;

            _context.Add(new ClientSubscription()
            {
                TrackedMailId = mail.Id,
                UserEmail = model.Email,
                MailCode = model.Code
            });

            await _context.SaveChangesAsync();

            await _emailSender.SendEmailAsync(model.Email, "Postal Service Subscription Notification", "Your subscription was created successfully for the mail with code: " + model.Code);
            
            ViewBag.MailSent = true;
            
            return RedirectToAction(nameof(Track));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}