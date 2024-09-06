using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostalManagementMVC.Entities
{
    public class PostOffice
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 2)]
        public string Name { get; set; }

        [Required]
        [DisplayName("City")]
        public int CityId { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 2)]
        [DisplayName("Contact")]
        public string ContactDetails { get; set; }

        [ForeignKey(nameof(Location.PostOfficeInChargeId))]
        public virtual ICollection<Location>? LocationsInCharge { get; set; }
    }
}
