using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoor.Models.ViewModel;
using BooksOnDoor.Utility;
using BooksOnDoorWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;

namespace BooksOnDoorWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        public UserController(ApplicationDbContext db,UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult RoleManagement(string userId)
        {
            string roleId = _db.UserRoles.FirstOrDefault(x => x.UserId == userId).RoleId;
            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                ApplicationUser = _db.ApplicationUsers.Include(u => u.Company).FirstOrDefault(u => u.Id == userId),
                RoleList = _db.Roles.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Name
                }),
                CompanyList = _db.Companies.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            roleManagementVM.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;
            return View(roleManagementVM);
        }
        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            string roleId = _db.UserRoles.FirstOrDefault(x => x.UserId == roleManagementVM.ApplicationUser.Id).RoleId;
            string oldRole = _db.Roles.FirstOrDefault(x => x.Id == roleId).Name;
            if (!(roleManagementVM.ApplicationUser.Role == oldRole))
            {
                ApplicationUser appUser = _db.ApplicationUsers.FirstOrDefault(x => x.Id == roleManagementVM.ApplicationUser.Id);
                if (roleManagementVM.ApplicationUser.Role == SD.Role_Company)
                {
                    appUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if(oldRole == SD.Role_Company)
                {
                    appUser.CompanyId = null;
                }
                _db.SaveChanges();
                _userManager.RemoveFromRoleAsync(appUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(appUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }

            return RedirectToAction(nameof(Index));
        }

        #region Api Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> applicationUsers = _db.ApplicationUsers.Include(u => u.Company).ToList();
            var userRole = _db.UserRoles.ToList();
            var user = _db.Roles.ToList();
            foreach (var users in applicationUsers)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == users.Id).RoleId;
                users.Role = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;

                if (users.Company == null)
                {
                    users.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = applicationUsers });
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var appUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (appUser == null)
                return Json(new { success = false, Message = "Error while Locking/Unlocking!!" });
            if (appUser.LockoutEnd != null && appUser.LockoutEnd > DateTime.Now)
            {
                appUser.LockoutEnd = DateTime.Now;
                TempData["Success"] = "Unlocked Successfully";
            }
            else
            {
                appUser.LockoutEnd = DateTime.Now.AddYears(1000);
                TempData["Success"] = "Locked Successfully";
            }
            _db.SaveChanges();
            return Json(new { success = true, Message = "Successful!!" });
        }
        #endregion 
    }
}
