using PostalManagementMVC.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace PostalManagementMVC.Entities
{
    public class City : IPrimaryAttributes
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 2)]
        public string Name { get; set; }


        [Required]
        public decimal Longitude { get; set; }
        

        [Required]
        public decimal Latitude { get; set; }

        [Required]
        public int CountryId { get; set; }



        [ForeignKey(nameof(PostOffice.CityId))]
        public virtual ICollection<PostOffice>? PostOffices { get; }
        //[NotMapped]
        //public Country Country { get; set; }
    }
}
