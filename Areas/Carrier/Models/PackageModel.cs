using PostalManagementMVC.Entities;

namespace PostalManagementMVC.Areas.Carrier.Models
{
    public class PackageModel
    {
        public Mail Bundle { get; set; }
        public List<Mail>? PackageItems { get; set; }
    }
}
