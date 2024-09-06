using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Interfaces;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PostalManagementMVC.Data
{

    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(255, MinimumLength = 2)]
        [DisplayName("First Name")]
        public String FirstName { get; set; }


        [Required]
        [StringLength(255, MinimumLength = 2)]
        [DisplayName("Last Name")]
        public String LastName { get; set; }


        [DisplayName("Active From")]
        public DateTime ActiveFrom { get; set; }


        [DisplayName("Active To")]
        public DateTime? ActiveTo { get; set; }

        [DisplayName("Location Assigned")]
        public int? LocationAssignedId { get; set; }

        [NotMapped]
        [DisplayName("Location Assigned")]
        public string? LocationAssignedName { get; set; }

        [NotMapped]
        public string? InitialRoleId { get; set; }

        [ForeignKey(nameof(MailStatus.OwnerId))]
        public virtual ICollection<MailStatus>? MailStatuses { get; set; }


        [ForeignKey(nameof(Mail.CreatedById))]
        public virtual ICollection<Mail>? CreatedMails { get; set; }


        [ForeignKey(nameof(Mail.ModifiedById))]
        public virtual ICollection<Mail>? ModifiedMails { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Country> Country { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<PostOffice> PostOffice { get; set; }

        public DbSet<Location> Location { get; set; }
        public DbSet<LocationConnection> LocationConnection { get; set; }

        public DbSet<Mail> Mail { get; set; }
        public DbSet<MailStatus> MailStatus { get; set; }
        public DbSet<StatusCatalog> StatusCatalog { get; set; }
        public DbSet<MailCategory> MailCategory { get; set; }

        public DbSet<PostalCode> PostalCode { get; set; }
        public DbSet<ClientSubscription> ClientSubscription { get; set; }
    }
}