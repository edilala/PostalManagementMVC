using PostalManagementMVC.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostalManagementMVC.Entities
{
    public class MailCategory : IPrimaryAttributes
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 2)]
        public string Name { get; set; }

        [ForeignKey(nameof(Mail.CategoryId))]
        public virtual ICollection<Mail>? Mails { get; set; }

        [Required]
        public double Fee { get; set; }
    }
}
