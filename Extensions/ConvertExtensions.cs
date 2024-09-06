using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using PostalManagementMVC.Data;
using PostalManagementMVC.Entities;
using PostalManagementMVC.Interfaces;

namespace PostalManagementMVC.Extensions
{
    public static class ConvertExtensions
    {
        public static List<SelectListItem> ConvertToSelectList<T>(this IEnumerable<T> collection, int selectedValue) where T : IPrimaryAttributes
        {
            return (from item in collection
                    select new SelectListItem
                    {
                        Text = item.Name,
                        Value = item.Id.ToString(),
                        Selected = (item.Id == selectedValue)
                    }).ToList();
        }

        public static List<SelectListItem> ConvertMailCategoriesToSelectList<T>(this IEnumerable<MailCategory> collection, int selectedValue) where T : IPrimaryAttributes
        {
            return (from item in collection
                    select new SelectListItem
                    {
                        Text = $"{item.Name} ({item.Fee}Lek)",
                        Value = item.Id.ToString(),
                        Selected = (item.Id == selectedValue)
                    }).ToList();
        }

        public static List<SelectListItem> ConvertUsersToSelectList(this IEnumerable<ApplicationUser> collection, string selectedValue)
        {
            if (collection == null) return new List<SelectListItem>();
            return (from item in collection
                    select new SelectListItem
                    {
                        Text = item.UserName,
                        Value = item.Id,
                        Selected = String.Equals(item.Id, selectedValue, StringComparison.OrdinalIgnoreCase)
                    }).ToList();
        }
        public static List<SelectListItem> ConvertRolesToSelectList(this IEnumerable<IdentityRole> collection, string selectedValue)
        {
            if (collection == null) return new List<SelectListItem>();
            return (from item in collection
                    select new SelectListItem
                    {
                        Text = item.Name,
                        Value = item.Name,
                        Selected = String.Equals(item.Name, selectedValue, StringComparison.OrdinalIgnoreCase)
                    }).ToList();
        }

        public static List<SelectListItem> ConvertPostalCodesToSelectList(this IEnumerable<PostalCode> collection, int selectedValue)
        {
            if (collection == null) return new List<SelectListItem>();
            return (from item in collection
                    select new SelectListItem
                    {
                        Text = item.ZipCode,
                        Value = item.Id.ToString(),
                        Selected = (item.Id == selectedValue)
                    }).ToList();
        }
    }
}
