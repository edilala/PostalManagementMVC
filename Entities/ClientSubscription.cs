using System.ComponentModel.DataAnnotations;

namespace PostalManagementMVC.Entities
{
    public class ClientSubscription
    {
        public int Id { get; set; }

        [StringLength(255)]
        [Required]
        [EmailAddress]
        public string UserEmail { get; set; }

        [StringLength(450)]
        [Required]
        public string MailCode { get; set; }


        public int TrackedMailId { get; set; }
    }
}
