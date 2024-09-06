using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostalManagementMVC.Entities
{
    public class Country
    {
        public int Id { get; set; }


        [Required]
        [StringLength(255, MinimumLength = 2)]
        public string Name { get; set; }


        [ForeignKey(nameof(City.CountryId))]
        public virtual ICollection<City>? Cities { get; set; }
    }
}
