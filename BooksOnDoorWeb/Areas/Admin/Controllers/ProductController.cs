using BooksOnDoor.DataAccess.Repository.IRepository;
using BooksOnDoor.Models.Models;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product obj)
        {
            if(ModelState.IsValid)
            {
                _unitOfWork.Product.Add(obj);
                _unitOfWork.save();
                TempData["success"] = "Product Created Successfully";
                return RedirectToAction("Index","Product");
            }
            return View();
        }
        public IActionResult Edit(int? Id)
        {
            Product productFromDb = _unitOfWork.Product.Get(u=>u.Id == Id);
            if(productFromDb == null|| Id==0||Id==null)
            {
                return NotFound();
            }
            return View(productFromDb);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product obj)
        {
            if(ModelState.IsValid)
            {
                _unitOfWork.Product.update(obj);
                _unitOfWork.save();
                TempData["success"] = "Product updated!!";
                return RedirectToAction("Index", "Product");
            }
            return View(obj);
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
