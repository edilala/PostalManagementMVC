using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostalManagementMVC.Entities
{
    public class LocationConnection
    {
        public int Id { get; set; }

        [Required]
        public int FromLocationId { get; set; }
        
        
        [Required]
        public int ToLocationId { get; set; }

        [NotMapped]
        [DisplayName("From Location")]
        public string? FromLocationName { get; set; }

        [NotMapped]
        [DisplayName("To Location")]
        public string? ToLocationName { get; set; }
        
        
        [Required]
        [StringLength(20)]
        [DisplayName("At Day")]
        public string OnDay { get; set; }

        [NotMapped]
        public int OnDayIndex { get; set; }

        [NotMapped]
        public double TimeCost { get; set; }

        [Required]
        [DisplayName("In Transport Days")]
        public int TransportDays { get; set; }

        [Required]
        public double Cost { get; set; }
    }
}
