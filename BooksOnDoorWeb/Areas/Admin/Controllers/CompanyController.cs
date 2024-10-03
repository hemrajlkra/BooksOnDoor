using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoor.Models.ViewModel;
using BooksOnDoor.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;

namespace BooksOnDoorWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork= unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> CompanyList = _unitOfWork.Company.Getall().ToList();
            
            return View(CompanyList);
        }
        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0) 
            { 
                //create
                return View(new Company());
            }
            else
            {
                //update
                Company companyobj= _unitOfWork.Company.Get(u => u.Id == id); 
                return View(companyobj);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(Company companyobj)
        {
            if(ModelState.IsValid)
            {
                if(companyobj.Id == 0|| companyobj.Id == null) 
                {
                    _unitOfWork.Company.Add(companyobj);
                    TempData["success"] = "Product Created Successfully";
                }
                else
                {
                    _unitOfWork.Company.update(companyobj);
                    TempData["success"] = "Product Updated Successfully";
                }
                _unitOfWork.save();
                //TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                
                return View(companyobj);
            }
        }
        #region Api Calls
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> prod = _unitOfWork.Company.Getall().ToList();
            return Json(new { data =prod });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var companyToBeDeleted = _unitOfWork.Company.Get(u=>u.Id == id);
            if(companyToBeDeleted == null)
                return Json(new {success = false, Message="Error while deleting Id not found!!"});
            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.save();
            return Json(new { success = true, Message = "Deleted Successfully!!" });
        }
        #endregion 
    }
}
