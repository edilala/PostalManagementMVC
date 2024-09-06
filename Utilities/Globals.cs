using Humanizer;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PostalManagementMVC.Utilities
{
    public class Globals
    {
        // STATUSES
        public const string REGISTERED = "REGISTERED";
        public const string PICK_UP = "PICK-UP";
        public const string IN_TRANSIT = "IN TRANSIT";
        public const string RETURNED = "RETURNED";
        public const string OUT_FOR_DELIVERY = "OUT FOR DELIVERY";
        public const string DELIVERED = "DELIVERED";
        public const string CANCELLED = "CANCELLED";
        public const string DELIVERED_TO_TRANSIT_CENTER = "DELIVERED TO TRANSIT CENTER";
        public const string DELIVERED_TO_DESTINATION_CENTER = "DELIVERED TO DESTINATION CENTER";

        // CATEGORIES
        public const string BUNDLE_CATEGORY = "BUNDLE";


        public static readonly string[] WEEK_DAYS_ARR = new string[] { "MONDAY", "TUESDAY", "WEDNESDAY", "THURSDAY", "FRIDAY", "SATURDAY", "SUNDAY" };
    }
}
