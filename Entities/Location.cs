using PostalManagementMVC.Data;
using PostalManagementMVC.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostalManagementMVC.Entities
{
    public class Location : IPrimaryAttributes
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 2)]
        public string Name { get; set; }


        [Required]
        [DisplayName("Post Office In Charge")]
        public int PostOfficeInChargeId { get; set; }

        [NotMapped]
        [DisplayName("Default Postal Code")]
        public int? InitialPostalCodeId { get; set; }



        [DisplayName("Is Post Office")]
        public bool IsPostOffice { get; set; }

        [ForeignKey(nameof(LocationConnection.ToLocationId))]
        public virtual ICollection<LocationConnection>? ToLocations { get; set; }



        [ForeignKey(nameof(LocationConnection.FromLocationId))]
        public virtual ICollection<LocationConnection>? FromLocations { get; set; }


        [ForeignKey(nameof(Mail.StartLocationId))]
        public virtual ICollection<Mail>? MailLocationsStart { get; set; }



        [ForeignKey(nameof(Mail.EndLocationId))]
        public virtual ICollection<Mail>? MailLocationsEnd { get; set; }



        [ForeignKey(nameof(ApplicationUser.LocationAssignedId))]
        public virtual ICollection<ApplicationUser>? Employees { get; set; }

        [ForeignKey(nameof(PostalCode.LocationId))]
        public virtual ICollection<PostalCode>? ZipCodes { get; set; }

    }
}
