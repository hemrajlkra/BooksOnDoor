using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using BooksOnDoor.Models.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.Generic;

namespace BooksOnDoorWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductController(IUnitOfWork unitOfWork)
        {
                _unitOfWork= unitOfWork;
        }
        public IActionResult Index()
        {
            List<Product> products = _unitOfWork.Product.Getall().ToList();
            
            return View(products);
        }
        public IActionResult Upsert(int? id)
        {
            //ViewBag.CategoryList = CategoryList;
            
            ProductVM productVM = new()
            {
                CategoryList = _unitOfWork.Category.Getall().
                Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() }),
                Product = new Product()
            };
            if (id == null || id == 0) 
            { 
                //create
                return View(productVM);
            }
            else
            {
                //update
                productVM.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(productVM);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Upsert(ProductVM productVM,IFormFile file)
        {
            if(ModelState.IsValid)
            {
                _unitOfWork.Product.Add(productVM.Product);
                _unitOfWork.save();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index");
            }
            else
            {
                productVM.CategoryList = _unitOfWork.Category.Getall().
                Select(u => new SelectListItem { Text = u.Name, Value = u.Id.ToString() });
                return View(productVM);
            }
        }
        public IActionResult Delete(int? Id)
        {
            Product productFromDb = _unitOfWork.Product.Get(u=>u.Id==Id);
            if (Id == 0 || Id == null || productFromDb == null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public IActionResult DeleteItem(int? Id) 
        {
            if (ModelState.IsValid)
            {
                Product productFromDb = _unitOfWork.Product.Get(u => u.Id == Id);
                _unitOfWork.Product.Remove(productFromDb);
                _unitOfWork.save();
                TempData["success"] = "Product Deleted!!";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
