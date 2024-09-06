using Microsoft.AspNetCore.Identity;
using PostalManagementMVC.Data;

namespace PostalManagementMVC.Areas.Admin.Models
{
    public class UserRolesModel
    {
        public ApplicationUser User { get; set; }
        public IEnumerable<IdentityRole>? PossessedRoles { get; set; }
        public IEnumerable<IdentityRole>? OtherRoles { get; set; }
    }
}
