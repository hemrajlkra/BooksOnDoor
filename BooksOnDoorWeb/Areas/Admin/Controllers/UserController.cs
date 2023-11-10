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
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;
        public UserController(UserManager<IdentityUser> userManager,IUnitOfWork unitofWork,RoleManager<IdentityRole> roleManager)
        {   
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitofWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult RoleManagement(string userId)
        {
            RoleManagementVM roleManagementVM = new RoleManagementVM()
            {
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == userId, includeProperties: "Company"),
                RoleList = _roleManager.Roles.Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Name
                }),
                CompanyList = _unitOfWork.Company.Getall().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                })
            };
            roleManagementVM.ApplicationUser.Role =_userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u=>u.Id==userId))
                .GetAwaiter().GetResult().FirstOrDefault();
            return View(roleManagementVM);
        }
        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {
            string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == roleManagementVM.ApplicationUser.Id))
                .GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser appUser = _unitOfWork.ApplicationUser.Get(x => x.Id == roleManagementVM.ApplicationUser.Id);
            if (!(roleManagementVM.ApplicationUser.Role == oldRole))
            {
                
                if (roleManagementVM.ApplicationUser.Role == SD.Role_Company)
                {
                    appUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if(oldRole == SD.Role_Company)
                {
                    appUser.CompanyId = null;
                }
                _unitOfWork.ApplicationUser.Update(appUser);
                _unitOfWork.save();
                _userManager.RemoveFromRoleAsync(appUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(appUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }

            return RedirectToAction(nameof(Index));
        }

        #region Api Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> applicationUsers = _unitOfWork.ApplicationUser.Getall(includeProperties:"Company").ToList();
            
            foreach (var users in applicationUsers)
            {
                users.Role = _userManager.GetRolesAsync(users).GetAwaiter().GetResult().FirstOrDefault();

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
            var appUser = _unitOfWork.ApplicationUser.Get(u => u.Id == id);
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
            _unitOfWork.ApplicationUser.Update(appUser);
            _unitOfWork.save();
            return Json(new { success = true, Message = "Successful!!" });
        }
        #endregion 
    }
}
