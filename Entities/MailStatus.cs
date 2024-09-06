using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace PostalManagementMVC.Entities
{
    public class MailStatus
    {
        public int Id { get; set; }

        [Required]
        public int MailId { get; set; }

        [NotMapped]
        public string? StatusName { get; set; }


        [Required]
        public int StatusCatalogId { get; set; }

        [StringLength(2000)]
        public string? Note { get; set; }

        [Required]
        public string OwnerId { get; set; }

        [NotMapped]
        public string? OwnerName { get; set; }

        [Required]
        public DateTime TimeAssigned { get; set; }


    }
}
