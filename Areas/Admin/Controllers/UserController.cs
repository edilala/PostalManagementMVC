using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PostalManagementMVC.Areas.Admin.Models;
using PostalManagementMVC.Data;
using PostalManagementMVC.Extensions;
using PostalManagementMVC.Interfaces;
using PostalManagementMVC.Utilities;
using System.Data;

namespace PostalManagementMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]

    public class UserController : AbstractController
    {
        public readonly UserManager<ApplicationUser> _userManager;

        public UserController(ApplicationDbContext context, IEmailSender emailSender, UserManager<ApplicationUser> userManager) : base(context, emailSender)
        {
            _userManager = userManager;
        }

        // GET: Users - shfaq listen e perdoruesve nga databaza
        public async Task<IActionResult> Index()
        {
            return View(await (from users in _context.Users
                               join locations in _context.Location
                               on users.LocationAssignedId equals locations.Id
                               select new ApplicationUser()
                               {
                                   Id = users.Id,
                                   FirstName = users.FirstName,
                                   LastName = users.LastName,
                                   UserName = users.UserName,
                                   NormalizedEmail = users.NormalizedEmail,
                                   NormalizedUserName = users.NormalizedUserName,
                                   LocationAssignedId = locations.Id,
                                   LocationAssignedName = locations.Name,
                                   ActiveFrom = users.ActiveFrom,
                                   ActiveTo = users.ActiveTo,
                               }
                               ).ToListAsync());
        }

        // GET: Details - kontrollon nese kemi ne databaze user me kte id
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _helper.GetUserWithLocationById(id);
            if (user == null)
            {
                return NotFound();
            }


            return View(user);
        }

        // GET: Edit - editimi i nje useri egziztues ne databaze
        public async Task<IActionResult> Edit(string id)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.LocationsList = (await _context.Location.ToListAsync()).ConvertToSelectList(-1);

            return View(user);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id, FirstName, LastName, Email, UserName, LocationAssignedId")] ApplicationUser user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var userToUpdate = await _userManager.FindByIdAsync(user.Id);
                    if(userToUpdate == null) {
                        return NotFound();
                    }
                    userToUpdate.FirstName = user.FirstName;
                    userToUpdate.LastName = user.LastName;
                    userToUpdate.Email = user.Email;
                    userToUpdate.UserName = user.UserName;
                    userToUpdate.LocationAssignedId = user.LocationAssignedId;

                    var updateRes = await _userManager.UpdateAsync(userToUpdate);
                    if(updateRes.Errors.Count() > 0)
                    {
                        foreach(var error in updateRes.Errors)
                        {
                            ModelState.AddModelError(error.Code, error.Description);
                        }
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                if(ModelState.ErrorCount == 0)
                    return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        // GET: Disable - diaktivizon nje perdorues
        public async Task<IActionResult> Disable(string id)
        {
            if (id == null || _context.Users == null)
            {
                return NotFound();
            }

            var user = await _helper.GetUserWithLocationById(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

       
        [HttpPost, ActionName("Disable")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.Users == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Users'  is null.");
            }
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Roles - shfaq te dhenat per rolet e cdo useri
        public async Task<IActionResult> Roles(string id)
        {
            UserRolesModel userRoles = new UserRolesModel()
            {
                User = await _context.Users.FindAsync(id)
            };

            userRoles.PossessedRoles = await (from userRole in _context.UserRoles
                                              where userRole.UserId == id
                                              join roles in _context.Roles
                                              on userRole.RoleId equals roles.Id
                                              select new IdentityRole
                                              {
                                                  Id = roles.Id,
                                                  Name = roles.Name,
                                                  NormalizedName = roles.NormalizedName,
                                                  ConcurrencyStamp = roles.ConcurrencyStamp,
                                              }
                                              ).ToListAsync();

            //userRoles.OtherRoles = await _context.Roles.Where(r => userRoles.PossessedRoles.Any(p => p.Id == r.Id)).ToListAsync();

            var possessedRoleIds = userRoles.PossessedRoles.Select(p => p.Id).ToList();
            userRoles.OtherRoles = await _context.Roles
                .Where(rol => !possessedRoleIds.Contains(rol.Id))
                .ToListAsync();
            return View(userRoles);
        }

        public async Task<IActionResult> AddRole(string userId, string roleId)
        {
            if (String.IsNullOrWhiteSpace(userId) || String.IsNullOrWhiteSpace(roleId))
                return BadRequest();

            if (_context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
                return NotFound();

            ViewBag.RoleName = role.Name;

            return View(new IdentityUserRole<string>() { RoleId = roleId, UserId = userId});

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole([Bind("UserId,RoleId")] IdentityUserRole<string> userRole)
        {
            try
            {
                // check if user and role exists
                var user = await _context.Users.FindAsync(userRole.UserId);
                if (user == null)
                    return NotFound();
                var role = await _context.Roles.FindAsync(userRole.RoleId);
                if (role == null)
                    return NotFound();

                IdentityUserRole<string> newUserRole = new IdentityUserRole<string>()
                {
                    UserId = userRole.UserId,
                    RoleId = userRole.RoleId,
                };

                _context.Add(newUserRole);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return RedirectToAction(nameof(Roles), new { id = userRole.UserId });
        }


        public async Task<IActionResult> RemoveRole(string userId, string roleId)
        {
            if (String.IsNullOrWhiteSpace(userId) || String.IsNullOrWhiteSpace(roleId))
                return BadRequest();

            if (_context.Users == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound();
            var role = await _context.Roles.FindAsync(roleId);
            if (role == null)
                return NotFound();

            ViewBag.RoleName = role.Name;

            return View(new IdentityUserRole<string>() { RoleId = roleId, UserId = userId });

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole([Bind("UserId,RoleId")] IdentityUserRole<string> userRole)
        {
            try
            {
                // check if user and role exists
                var userRoleToRemove = await _context.UserRoles.FirstOrDefaultAsync(r => r.UserId == userRole.UserId && r.RoleId == userRole.RoleId);

                if (userRoleToRemove != null)
                {
                    _context.UserRoles.Remove(userRoleToRemove);
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return RedirectToAction(nameof(Roles), new { id = userRole.UserId });
        }




        private bool UserExists(string id)
        {
            return (_context.Users?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
