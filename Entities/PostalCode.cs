using PostalManagementMVC.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostalManagementMVC.Entities
{
    public class PostalCode
    {
        public int Id { get; set; }

        [Required]
        [StringLength(10, MinimumLength = 2)]
        [DisplayName("Zip Code")]
        public string ZipCode { get; set; }

        [Required]
        public int LocationId { get; set; }

        [NotMapped]
        [DisplayName("Location Name")]
        public string? LocationName { get; set; }

        [ForeignKey(nameof(Mail.PostalCodeId))]
        public virtual ICollection<Mail>? Mails { get; set; }

    }
}
