using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoor.Models.ViewModel;
using BooksOnDoor.Utility;
using BooksOnDoorWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;

namespace BooksOnDoorWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        public UserController(ApplicationDbContext db)
        {
            _db= db;
        }
        public IActionResult Index()
        {
            
            return View();
        }
        public IActionResult RoleManagement(string userId)
        {
            return View();
        }

        #region Api Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> applicationUsers = _db.ApplicationUsers.Include(u=>u.Company).ToList();
            var userRole = _db.UserRoles.ToList();
            var user = _db.Roles.ToList();
            foreach(var users in applicationUsers)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == users.Id).RoleId;
                users.Role = _db.Roles.FirstOrDefault(u=>u.Id== roleId).Name;

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
        public IActionResult LockUnlock([FromBody]string id)
        {
            var appUser = _db.ApplicationUsers.FirstOrDefault(u=>u.Id == id);
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
