using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostalManagementMVC.Entities
{
    public class Mail
    {
        public int Id { get; set; }
        public int? MailBundleId { get; set; }

        [Display(Name = "Tracking code")]
        public Guid? Code { get; set; }

        [NotMapped]
        public Guid? BundleCode { get; set; }

        [Display(Name = "Recipient Address")]
        [Required]
        [StringLength(510)]
        public string RecipientAddress { get; set; }


        [Display(Name = "Sender Address")]
        [StringLength(510)]
        public string? SenderAddress { get; set; }

        [Display(Name = "Origin")]
        [Required]
        public int StartLocationId { get; set; }

        [Display(Name = "Origin")]
        [NotMapped]
        public string? StartLocationName { get; set; }

        [Display(Name = "Destination")]
        [Required]
        public int EndLocationId { get; set; }
        [Display(Name = "Destination")]
        [NotMapped]
        public string? EndLocationName { get; set; }

        [Display(Name = "Category")]
        [Required]
        public int CategoryId { get; set; }
        [NotMapped]
        public MailCategory? Category { get; set; }

        [NotMapped]
        public string? UserName { get; set; }

        [Display(Name = "Zip Code")]
        public int? PostalCodeId { get; set; }

        [Display(Name = "Time Inserted")]
        public DateTime TimeInserted { get; set; }

        [Display(Name = "Time Delivered")]
        public DateTime? TimeDelivered { get; set; }

        [Required]
        [Display(Name = "Created By")]
        public string CreatedById { get; set; }
        [Display(Name = "Modified By")]
        public string? ModifiedById { get; set; }

        [Required]
        public double Height { get; set; }

        [Required]
        public double Width { get; set; }

        [Required]
        public double Hight { get; set; }

        [Required]
        public double Weight { get; set; }

        [Display(Name = "Days To Delivery")]
        [Required]
        public double DaysToDelivery { get; set; }
        
        [Display(Name = "Overdue Days")]
        [NotMapped]
        public double OverdueDays { get; set; }


        [Display(Name = "Receiver Contact")]
        [Required]
        [StringLength(20, MinimumLength = 10)]
        public string ReceiverContactNr { get; set; }

        [Display(Name = "Choosen Path")]
        [StringLength(4000)]
        public string? ChoosenPath { get; set; }

        [NotMapped]
        public MailStatus? CurrentStatus { get; set; }
        
        [Display(Name = "Status Note")]
        [NotMapped]
        public string? CurrentStatusNote { get; set; }

        [NotMapped]
        public int? CityId { get; set; }
        [NotMapped]
        public string? CityName { get; set; }


        [ForeignKey(nameof(Mail.MailBundleId))]
        public virtual ICollection<Mail>? Mails { get; set; }

        [ForeignKey(nameof(MailStatus.MailId))]
        public virtual ICollection<MailStatus>? Statuses { get; set; }

        [ForeignKey(nameof(ClientSubscription.TrackedMailId))]
        public virtual ICollection<ClientSubscription>? Subscriptions { get; set; }
    }
}
