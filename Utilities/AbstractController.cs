using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Interfaces;

namespace PostalManagementMVC.Utilities
{
    public class AbstractController: Controller
    {
        protected readonly ApplicationDbContext _context;
        protected readonly Helper _helper;
        protected IEmailSender _emailSender;

        public AbstractController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _helper = new Helper(context);
            _emailSender = emailSender;
        }

        internal async Task NotifyUserMailStatusChanged(int mailId, string newMailStatus)
        {
            bool isDev = false;
            if (isDev)
                return;

            if (mailId == 0 || String.IsNullOrWhiteSpace(newMailStatus))
                return;

            var subscribedEmailsForMail = await _context.ClientSubscription.Where(s => s.TrackedMailId == mailId).ToListAsync();

            if (subscribedEmailsForMail.Count == 0)
                return;

            string emailBody = "Dear client. Your mail status is updated into: " + newMailStatus;

            foreach(var subscription in subscribedEmailsForMail)
            {
                try
                {
                    await _emailSender.SendEmailAsync(subscription.UserEmail, "Postal Service Subscription Notification", emailBody + ". This email is relative to your mail with the following code: " + subscription.MailCode);
                }
                catch(Exception) {
                    // exception handling
                }
            }
        }
    }
}
