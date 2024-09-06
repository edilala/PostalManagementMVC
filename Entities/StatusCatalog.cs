using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostalManagementMVC.Entities
{
    public class StatusCatalog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 2)]
        public string Name { get; set; }

        [ForeignKey(nameof(MailStatus.StatusCatalogId))]
        public virtual ICollection<MailStatus> MailStatuses { get; set; }
    }
}
